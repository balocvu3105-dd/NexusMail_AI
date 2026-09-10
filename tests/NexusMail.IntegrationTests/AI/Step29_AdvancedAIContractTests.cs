using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using FluentAssertions;
using NexusMail.Application.Abstractions.AI;
using NexusMail.Domain.AI.Entities;
using NexusMail.Domain.AI.Enums;
using NexusMail.IntegrationTests.Email;
using NexusMail.Application.Features.AI.Commands.GenerateSummary;
using NexusMail.Application.Features.AI.Commands.CategorizeEmail;
using NexusMail.Application.Features.AI.Commands.ScorePriority;
using NexusMail.Application.Features.AI.Commands.GenerateEmbedding;
using MediatR;
using NexusMail.Shared.Domain;
using NexusMail.Shared.Common;
using NexusMail.Application.Features.Email.Abstractions;

namespace NexusMail.IntegrationTests.AI;

public class Step29_AdvancedAIContractTests : IntegrationTestBase
{
    private readonly Mock<ISummaryService> _summaryServiceMock;
    private readonly Mock<IClassificationService> _classificationServiceMock;
    private readonly Mock<IPriorityService> _priorityServiceMock;
    private readonly Mock<IEmbeddingService> _embeddingServiceMock;

    public Step29_AdvancedAIContractTests()
    {
        _summaryServiceMock = new Mock<ISummaryService>();
        _classificationServiceMock = new Mock<IClassificationService>();
        _priorityServiceMock = new Mock<IPriorityService>();
        _embeddingServiceMock = new Mock<IEmbeddingService>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISummaryService>(_ => _summaryServiceMock.Object);
        services.AddScoped<IClassificationService>(_ => _classificationServiceMock.Object);
        services.AddScoped<IPriorityService>(_ => _priorityServiceMock.Object);
        services.AddScoped<IEmbeddingService>(_ => _embeddingServiceMock.Object);
    }

    private async Task<(Guid workspaceId, Guid accountId, Guid emailId)> SeedEmailAsync()
    {
        var dbContext = DbContext;
        var workspace = NexusMail.Domain.Workspace.Entities.Workspace.Create($"Test WS {Guid.NewGuid()}", Guid.NewGuid());
        dbContext.Workspaces.Add(workspace);
        var account = NexusMail.Domain.Email.Entities.EmailAccount.Create(workspace.Id, NexusMail.Domain.Email.Enums.EmailProvider.Google, $"test_{Guid.NewGuid()}@test.com", "valid", "refresh", DateTime.UtcNow.AddHours(1));
        dbContext.EmailAccounts.Add(account);
        var email = new NexusMail.Domain.Email.Entities.Email(account.Id, workspace.Id, $"msg_{Guid.NewGuid()}", "sender@test.com", "Subject", "Body", DateTimeOffset.UtcNow);
        dbContext.Emails.Add(email);
        await dbContext.SaveChangesAsync();
        return (workspace.Id, account.Id, email.Id);
    }

    [Fact]
    public async Task DuplicatePriorityRequest_ShouldNotRegenerate()
    {
        var mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        var dbContext = DbContext;
        var (workspaceId, accountId, emailId) = await SeedEmailAsync();

        var analysis = AIAnalysis.Create(emailId);
        analysis.StartPriority(Guid.NewGuid());
        analysis.CompletePriority(analysis.PriorityAttemptId!.Value, 90, "High");
        dbContext.Set<AIAnalysis>().Add(analysis);
        await dbContext.SaveChangesAsync();

        _priorityServiceMock.Setup(x => x.GeneratePriorityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new AIPriorityResult(10, "Low", false)));

        var command = new ScorePriorityCommand(emailId, workspaceId);
        var result = await mediator.Send(command);

        result.IsSuccess.Should().BeTrue();
        _priorityServiceMock.Verify(x => x.GeneratePriorityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        var savedAnalysis = await dbContext.Set<AIAnalysis>().FirstOrDefaultAsync(a => a.EmailId == emailId);
        savedAnalysis!.Priority.Should().Be("High");
    }

    [Fact]
    public async Task DuplicateClassificationRequest_ShouldNotRegenerate()
    {
        var mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        var dbContext = DbContext;
        var (workspaceId, accountId, emailId) = await SeedEmailAsync();

        var analysis = AIAnalysis.Create(emailId);
        analysis.StartClassification(Guid.NewGuid());
        analysis.CompleteClassification(analysis.ClassificationAttemptId!.Value, "Invoice", 0.99, new List<string> { "Finance" });
        dbContext.Set<AIAnalysis>().Add(analysis);
        await dbContext.SaveChangesAsync();

        _classificationServiceMock.Setup(x => x.GenerateClassificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new AIClassificationResult("Spam", 0.9, new List<string>())));

        var command = new CategorizeEmailCommand(emailId, workspaceId);
        var result = await mediator.Send(command);

        result.IsSuccess.Should().BeTrue();
        _classificationServiceMock.Verify(x => x.GenerateClassificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        var savedAnalysis = await dbContext.Set<AIAnalysis>().FirstOrDefaultAsync(a => a.EmailId == emailId);
        savedAnalysis!.Category.Should().Be("Invoice");
    }

    [Fact]
    public async Task DuplicateEmbeddingRequest_ShouldNotRegenerate()
    {
        var mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        var dbContext = DbContext;
        var (workspaceId, accountId, emailId) = await SeedEmailAsync();

        var analysis = AIAnalysis.Create(emailId);
        analysis.StartEmbedding(Guid.NewGuid());
        analysis.CompleteEmbedding(analysis.EmbeddingAttemptId!.Value);
        dbContext.Set<AIAnalysis>().Add(analysis);
        await dbContext.SaveChangesAsync();

        _embeddingServiceMock.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<float[]>(new float[] { 0.1f }));

        var command = new GenerateEmbeddingCommand(emailId, workspaceId);
        var result = await mediator.Send(command);

        result.IsSuccess.Should().BeTrue();
        _embeddingServiceMock.Verify(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AIRequest_ShouldNotAccessAnotherWorkspaceEmail()
    {
        var mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        var dbContext = DbContext;
        
        var (workspaceA, _, _) = await SeedEmailAsync();
        var (workspaceB, _, emailBId) = await SeedEmailAsync();

        // Workspace A tries to access Email B
        var command = new GenerateSummaryCommand(emailBId, workspaceA);
        var result = await mediator.Send(command);

        // Assert guardrail 7 & 10
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Email.NotFound");

        // Verify no leakage or mutation
        var analysis = await dbContext.Set<AIAnalysis>().FirstOrDefaultAsync(a => a.EmailId == emailBId);
        analysis.Should().BeNull("No AIAnalysis should be created for unauthorized cross-workspace request.");
    }

    [Fact]
    public async Task ConcurrentAIInitialization_ShouldCreateSingleAnalysis()
    {
        var (workspaceId, accountId, emailId) = await SeedEmailAsync();
        
        _summaryServiceMock.Setup(x => x.GenerateSummaryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("Summary Concurrency"));
            
        _priorityServiceMock.Setup(x => x.GeneratePriorityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new AIPriorityResult(100, "Urgent", true)));

        var barrier = new Barrier(2);

        var taskA = Task.Run(async () =>
        {
            using var scope1 = ServiceProvider.CreateScope();
            var mediator1 = scope1.ServiceProvider.GetRequiredService<IMediator>();
            barrier.SignalAndWait(TimeSpan.FromSeconds(5));
            var command = new GenerateSummaryCommand(emailId, workspaceId);
            try { return await mediator1.Send(command); }
            catch (AIProviderTransientException) 
            { 
                // In actual execution, MassTransit retries this in a new scope
                using var scope2 = ServiceProvider.CreateScope();
                var mediator2 = scope2.ServiceProvider.GetRequiredService<IMediator>();
                return await mediator2.Send(command);
            }
        });

        var taskB = Task.Run(async () =>
        {
            using var scope1 = ServiceProvider.CreateScope();
            var mediator1 = scope1.ServiceProvider.GetRequiredService<IMediator>();
            barrier.SignalAndWait(TimeSpan.FromSeconds(5));
            var command = new ScorePriorityCommand(emailId, workspaceId);
            try { return await mediator1.Send(command); }
            catch (AIProviderTransientException)
            {
                using var scope2 = ServiceProvider.CreateScope();
                var mediator2 = scope2.ServiceProvider.GetRequiredService<IMediator>();
                return await mediator2.Send(command);
            }
        });

        await Task.WhenAll(taskA, taskB);

        taskA.Result.IsSuccess.Should().BeTrue();
        taskB.Result.IsSuccess.Should().BeTrue();

        // Evidence
        var dbContext = DbContext;
        var analyses = await dbContext.Set<AIAnalysis>().Where(a => a.EmailId == emailId).ToListAsync();
        analyses.Should().HaveCount(1, "Unique constraint should prevent duplicates and MassTransit retry handles transient failure.");
        
        var analysis = analyses.First();
        analysis.SummaryStatus.Should().Be(AIProcessingStatus.Succeeded);
        analysis.PriorityStatus.Should().Be(AIProcessingStatus.Succeeded);
    }

    [Fact]
    public async Task ConcurrentCapabilities_ShouldPersistIndependently()
    {
        var (workspaceId, accountId, emailId) = await SeedEmailAsync();
        
        // Pre-create the analysis so it's not Initialization Concurrency but Capability Concurrency
        var analysis = AIAnalysis.Create(emailId);
        DbContext.Set<AIAnalysis>().Add(analysis);
        await DbContext.SaveChangesAsync();
        
        _summaryServiceMock.Setup(x => x.GenerateSummaryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("Summary Concurrency"));
            
        _priorityServiceMock.Setup(x => x.GeneratePriorityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new AIPriorityResult(100, "Urgent", true)));

        var barrier = new Barrier(2);

        var taskA = Task.Run(async () =>
        {
            using var scope = ServiceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            barrier.SignalAndWait(TimeSpan.FromSeconds(5));
            var command = new GenerateSummaryCommand(emailId, workspaceId);
            return await mediator.Send(command);
        });

        var taskB = Task.Run(async () =>
        {
            using var scope = ServiceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            barrier.SignalAndWait(TimeSpan.FromSeconds(5));
            var command = new ScorePriorityCommand(emailId, workspaceId);
            return await mediator.Send(command);
        });

        await Task.WhenAll(taskA, taskB);

        taskA.Result.IsSuccess.Should().BeTrue();
        taskB.Result.IsSuccess.Should().BeTrue();

        using var assertScope = ServiceProvider.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<NexusMail.Infrastructure.Persistence.ApplicationDbContext>();
        
        var finalAnalysis = await assertDb.Set<AIAnalysis>().FirstOrDefaultAsync(a => a.EmailId == emailId);
        finalAnalysis.Should().NotBeNull();
        
        // Verify PostgreSQL column-level / MVCC update didn't lose state
        finalAnalysis!.SummaryStatus.Should().Be(AIProcessingStatus.Succeeded);
        finalAnalysis.PriorityStatus.Should().Be(AIProcessingStatus.Succeeded);
    }

    [Fact]
    public async Task FailedAIProcessing_ShouldNotMutateAutomationExecution()
    {
        // Guardrail 8: AI failure != Automation mutation
        var mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        var (workspaceId, accountId, emailId) = await SeedEmailAsync();

        // Clear existing outbox messages from seed data (e.g. EmailAccountConnectedEvent)
        var setupDb = Scope.ServiceProvider.GetRequiredService<NexusMail.Infrastructure.Persistence.ApplicationDbContext>();
        setupDb.Set<NexusMail.Application.Abstractions.Outbox.OutboxMessage>().RemoveRange(setupDb.Set<NexusMail.Application.Abstractions.Outbox.OutboxMessage>());
        await setupDb.SaveChangesAsync();

        // Cause a permanent failure
        _summaryServiceMock.Setup(x => x.GenerateSummaryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<string>(new Error("AI.Fail", "Failed to generate summary")));

        var command = new GenerateSummaryCommand(emailId, workspaceId);
        var result = await mediator.Send(command);

        result.IsSuccess.Should().BeTrue(); // Handler swallows permanent errors, but analysis is Failed

        using var assertScope = ServiceProvider.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<NexusMail.Infrastructure.Persistence.ApplicationDbContext>();
        
        var finalAnalysis = await assertDb.Set<AIAnalysis>().FirstOrDefaultAsync(a => a.EmailId == emailId);
        finalAnalysis.Should().NotBeNull();
        finalAnalysis!.SummaryStatus.Should().Be(AIProcessingStatus.Failed);

        // Verify that no OutboxMessages were created to trigger Automation (Domain events) after the failure
        var outboxMessages = await assertDb.Set<NexusMail.Application.Abstractions.Outbox.OutboxMessage>().ToListAsync();
        outboxMessages.Should().BeEmpty("Failed AI processing should not emit any domain events that might mutate automation.");
    }
}
