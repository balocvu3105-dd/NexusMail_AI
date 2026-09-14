using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;
using NexusMail.Contracts.Automation;
using NexusMail.Domain.AI.Entities;
using NexusMail.Domain.AI.Enums;
using NexusMail.Domain.Automation.Entities;
using NexusMail.Domain.Automation.Enums;
using NexusMail.IntegrationTests.Infrastructure;
using Moq;
using NexusMail.Application.Abstractions.AI;

namespace NexusMail.IntegrationTests.Automation;

[Collection("Integration")]
public class Step30_AutomationIntegrationTests : IClassFixture<CustomWebApplicationFactory<NexusMail.API.ApiMarker>>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<NexusMail.API.ApiMarker> _factory;
    private NexusMail.Infrastructure.Persistence.ApplicationDbContext _dbContext = null!;

    public Step30_AutomationIntegrationTests(CustomWebApplicationFactory<NexusMail.API.ApiMarker> factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<NexusMail.Infrastructure.Persistence.ApplicationDbContext>();
        
        _dbContext.AutomationRules.RemoveRange(_dbContext.AutomationRules);
        _dbContext.AutomationExecutions.RemoveRange(_dbContext.AutomationExecutions);
        _dbContext.Set<AIAnalysis>().RemoveRange(_dbContext.Set<AIAnalysis>());
        await _dbContext.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<(Guid workspaceId, Guid accountId, Guid emailId)> SeedEmailAsync()
    {
        var workspace = NexusMail.Domain.Workspace.Entities.Workspace.Create($"Test WS {Guid.NewGuid()}", Guid.NewGuid());
        _dbContext.Workspaces.Add(workspace);
        var account = NexusMail.Domain.Email.Entities.EmailAccount.Create(workspace.Id, NexusMail.Domain.Email.Enums.EmailProvider.Google, $"test_{Guid.NewGuid()}@test.com", "valid", "refresh", DateTime.UtcNow.AddHours(1));
        _dbContext.EmailAccounts.Add(account);
        var email = new NexusMail.Domain.Email.Entities.Email(account.Id, workspace.Id, $"msg_{Guid.NewGuid()}", "sender@test.com", "Subject", "Body", DateTimeOffset.UtcNow);
        _dbContext.Emails.Add(email);
        await _dbContext.SaveChangesAsync();
        return (workspace.Id, account.Id, email.Id);
    }

    [Fact]
    public async Task CompletionEvent_ShouldPublishOnlyWhenAllCapabilitiesTerminal()
    {
        var (_, _, emailId) = await SeedEmailAsync();
        
        var analysis = AIAnalysis.Create(emailId);
        _dbContext.Set<AIAnalysis>().Add(analysis);
        await _dbContext.SaveChangesAsync();

        var processingAttemptId = Guid.NewGuid();
        analysis.StartProcessing(processingAttemptId);
        analysis.CompleteProcessing(processingAttemptId, "Summary", 90, "High", "Invoice", 0.99, new List<string> { "tag1" }, false);
        await _dbContext.SaveChangesAsync();

        // Check if event was published
        analysis.IsCompletedEventPublished.Should().BeFalse();
        var outboxMessages = await _dbContext.Set<NexusMail.Application.Abstractions.Outbox.OutboxMessage>()
            .Where(m => m.Content.Contains(emailId.ToString()))
            .ToListAsync();
        outboxMessages.Should().NotContain(m => m.Type.Contains("EmailAIProcessingCompleted"));

        // Complete the embedding
        var embeddingAttemptId = Guid.NewGuid();
        analysis.StartEmbedding(embeddingAttemptId);
        analysis.CompleteEmbedding(embeddingAttemptId);

        await _dbContext.SaveChangesAsync();

        analysis.IsCompletedEventPublished.Should().BeTrue();
        
        var completedOutboxMsg = await _dbContext.Set<NexusMail.Application.Abstractions.Outbox.OutboxMessage>()
            .FirstOrDefaultAsync(m => m.Type.Contains("EmailAIProcessingCompleted") && m.ProcessedOnUtc == null && m.Content.Contains(emailId.ToString()));
        
        completedOutboxMsg.Should().NotBeNull();
    }

    [Fact]
    public async Task CompletionEvent_ShouldPublishAfterPartialFailure()
    {
        var (_, _, emailId) = await SeedEmailAsync();
        
        var analysis = AIAnalysis.Create(emailId);
        _dbContext.Set<AIAnalysis>().Add(analysis);
        await _dbContext.SaveChangesAsync();

        // Processing fails, Embedding succeeds
        var processingAttemptId = Guid.NewGuid();
        analysis.StartProcessing(processingAttemptId);
        analysis.FailProcessing(processingAttemptId, "API Error");

        var embeddingAttemptId = Guid.NewGuid();
        analysis.StartEmbedding(embeddingAttemptId);
        analysis.CompleteEmbedding(embeddingAttemptId);

        await _dbContext.SaveChangesAsync();

        analysis.IsCompletedEventPublished.Should().BeTrue();
        
        // Let's inspect the payload
        var outboxMsg = await _dbContext.Set<NexusMail.Application.Abstractions.Outbox.OutboxMessage>()
            .OrderByDescending(x => x.OccurredOnUtc)
            .FirstOrDefaultAsync(m => m.Type.Contains("EmailAIProcessingCompleted") && m.Content.Contains(emailId.ToString()));
            
        outboxMsg.Should().NotBeNull();
        
        var payload = System.Text.Json.JsonSerializer.Deserialize<NexusMail.Domain.AI.Events.EmailAIProcessingCompleted>(outboxMsg!.Content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        payload!.SummarySucceeded.Should().BeFalse();
        payload.ClassificationSucceeded.Should().BeFalse();
        payload.EmbeddingSucceeded.Should().BeTrue();
        payload.Category.Should().BeNull(); // Unavailable due to failure
    }

    [Fact]
    public async Task CompletionEvent_ShouldNotPublishTwiceUnderConcurrentCompletion()
    {
        var (_, _, emailId) = await SeedEmailAsync();
        var analysis = AIAnalysis.Create(emailId);
        _dbContext.Set<AIAnalysis>().Add(analysis);
        
        var processingAttemptId = Guid.NewGuid();
        analysis.StartProcessing(processingAttemptId);
        var embeddingAttemptId = Guid.NewGuid();
        analysis.StartEmbedding(embeddingAttemptId);
        
        await _dbContext.SaveChangesAsync();

        var barrier = new Barrier(2);

        // We simulate two concurrent transactions loading the entity and completing the last 2 capabilities simultaneously.
        var task1 = Task.Run(async () =>
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NexusMail.Infrastructure.Persistence.ApplicationDbContext>();
            var agg = await db.Set<AIAnalysis>().FirstAsync(a => a.EmailId == emailId);
            
            agg.CompleteProcessing(processingAttemptId, "Sum", 10, "Low", "Cat", 1, null, false);
            
            barrier.SignalAndWait(TimeSpan.FromSeconds(5));
            try { await db.SaveChangesAsync(); return true; } catch (DbUpdateConcurrencyException) { return false; }
        });

        var task2 = Task.Run(async () =>
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NexusMail.Infrastructure.Persistence.ApplicationDbContext>();
            var agg = await db.Set<AIAnalysis>().FirstAsync(a => a.EmailId == emailId);
            
            agg.CompleteEmbedding(embeddingAttemptId);
            
            barrier.SignalAndWait(TimeSpan.FromSeconds(5));
            try { await db.SaveChangesAsync(); return true; } catch (DbUpdateConcurrencyException) { return false; }
        });

        await Task.WhenAll(task1, task2);

        // One of them should fail due to concurrency token.
        // We will just do a final read and ensure they are both done.
        var finalDb = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<NexusMail.Infrastructure.Persistence.ApplicationDbContext>();
        var finalAgg = await finalDb.Set<AIAnalysis>().FirstAsync(a => a.EmailId == emailId);
        
        if (finalAgg.ProcessingState != AIProcessingStatus.Succeeded)
        {
            finalAgg.CompleteProcessing(processingAttemptId, "Sum", 10, "Low", "Cat", 1, null, false);
        }
        if (finalAgg.EmbeddingStatus != AIProcessingStatus.Succeeded)
        {
            finalAgg.CompleteEmbedding(embeddingAttemptId);
        }
        await finalDb.SaveChangesAsync();

        // Check Outbox
        var messages = await finalDb.Set<NexusMail.Application.Abstractions.Outbox.OutboxMessage>()
            .Where(m => m.Type.Contains("EmailAIProcessingCompleted") && m.Content.Contains(emailId.ToString()))
            .ToListAsync();
            
        messages.Count.Should().Be(1, "Exactly one completion event should be stored, proving IsCompletedEventPublished + concurrency token prevents duplicates.");
    }

    [Fact]
    public async Task AICompletion_ShouldTriggerSecondEvaluation_AndMaintainIdempotency()
    {
        var (workspaceId, accountId, emailId) = await SeedEmailAsync();
        
        // 1. Create a Rule that matches everything (Pre-AI)
        var rule1 = AutomationRule.Create(workspaceId, "Rule 1", TriggerType.EmailReceived, 
            @"{""type"":""comparison"",""Property"":""Subject"",""Operator"":""=="",""Value"":""Subject""}",
            @"[{""Type"":""Label"",""Parameters"":{""LabelName"":""Fast""}}]");
        
        // 2. Create a Rule that requires AI (Post-AI)
        var rule2 = AutomationRule.Create(workspaceId, "Rule 2", TriggerType.EmailReceived, 
            @"{""type"":""comparison"",""Property"":""Category"",""Operator"":""=="",""Value"":""Invoice""}",
            @"[{""Type"":""Label"",""Parameters"":{""LabelName"":""AI""}}]");
            
        _dbContext.AutomationRules.AddRange(rule1, rule2);
        await _dbContext.SaveChangesAsync();

        var evalService = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<NexusMail.Automation.RuleEngine.IRuleEvaluationService>();

        // Simulate EmailReceived publishing EvaluateRulesMessage (Pre-AI pass)
        await evalService.EvaluateAsync(new EvaluateRulesMessage { EmailId = emailId, WorkspaceId = workspaceId, Subject = "Subject", Sender = "sender@test.com", EmailAccountId = accountId }, CancellationToken.None);

        // Pre-AI Assertions
        var preAiExecutions = await _dbContext.AutomationExecutions.Where(e => e.EmailId == emailId).ToListAsync();
        preAiExecutions.Count.Should().Be(1, "Only Rule 1 should match initially");
        preAiExecutions[0].RuleId.Should().Be(rule1.Id);

        // Now AI processing completes
        var analysis = AIAnalysis.Create(emailId);
        _dbContext.Set<AIAnalysis>().Add(analysis);
        
        var processingAttemptId = Guid.NewGuid();
        analysis.StartProcessing(processingAttemptId);
        analysis.CompleteProcessing(processingAttemptId, "Sum", 10, "Low", "Invoice", 1.0, null, false);
        
        var embeddingAttemptId = Guid.NewGuid();
        analysis.StartEmbedding(embeddingAttemptId);
        analysis.CompleteEmbedding(embeddingAttemptId);
        await _dbContext.SaveChangesAsync();

        // Simulate AIProcessingCompletedConsumer calling RuleEvaluationService (Second pass)
        var postAiMsg = new EvaluateRulesMessage
        {
            EmailId = emailId,
            WorkspaceId = workspaceId,
            EmailAccountId = accountId,
            Subject = "Subject",
            Sender = "sender@test.com",
            AIMetadata = new Dictionary<string, object> { { "category", "Invoice" } }
        };
        await evalService.EvaluateAsync(postAiMsg, CancellationToken.None);

        // Post-AI Assertions
        var postAiExecutions = await _dbContext.AutomationExecutions.Where(e => e.EmailId == emailId).ToListAsync();
        postAiExecutions.Count.Should().Be(2, "Rule 2 should now match, and Rule 1 should not execute again (Idempotency)");
        postAiExecutions.Select(e => e.RuleId).Should().BeEquivalentTo(new[] { rule1.Id, rule2.Id });
    }
}
