using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexusMail.Application.Features.AI.Services;
using NexusMail.Domain.AI.Enums;
using NexusMail.Domain.Email.Entities;
using NexusMail.Infrastructure.Persistence;
using NexusMail.Application.Abstractions.AI;
using Xunit;

namespace NexusMail.IntegrationTests.Automation;

[Collection("Integration Tests")]
public class Step29_AIDecisioningTests : IClassFixture<CustomWebApplicationFactory<NexusMail.API.ApiMarker>>
{
    private readonly CustomWebApplicationFactory<NexusMail.API.ApiMarker> _factory;

    public Step29_AIDecisioningTests(CustomWebApplicationFactory<NexusMail.API.ApiMarker> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TestA_TransientFailure_ThrowsExceptionAndStaysPending()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var aiManager = scope.ServiceProvider.GetRequiredService<IAIWorkflowManager>();
        var fakeProvider = scope.ServiceProvider.GetRequiredService<FakeAIModelProvider>();

        var workspaceId = Guid.NewGuid();
        
        var workspace = NexusMail.Domain.Workspace.Entities.Workspace.Create("Test Workspace", Guid.NewGuid());
        var idProp = typeof(NexusMail.Shared.Domain.EntityBase<Guid>).GetProperty("Id");
        idProp?.SetValue(workspace, workspaceId);
        dbContext.Workspaces.Add(workspace);

        var accountId = Guid.NewGuid();
        var account = NexusMail.Domain.Email.Entities.EmailAccount.Create(workspaceId, NexusMail.Domain.Email.Enums.EmailProvider.Google, "test@example.com", "token", "refresh", null);
        // Force account ID
        idProp?.SetValue(account, accountId);
        
        dbContext.EmailAccounts.Add(account);
        var email = new NexusMail.Domain.Email.Entities.Email(accountId, workspaceId, "MessageId", "sender@example.com", "Test", "Body", DateTimeOffset.UtcNow);
        dbContext.Emails.Add(email);
        await dbContext.SaveChangesAsync();

        // Simulate transient failure
        fakeProvider.CompletionBehavior = request => throw new AIProviderTransientException("Simulated transient timeout");

        // Act & Assert
        await Assert.ThrowsAsync<AIProviderTransientException>(() => aiManager.ProcessEmailAIAsync(email.Id, workspaceId, default));

        // Verify state is back to Pending so MassTransit retry can pick it up
        var analysis = await dbContext.AIAnalyses.FirstOrDefaultAsync(x => x.EmailId == email.Id);
        Assert.NotNull(analysis);
        Assert.Equal(AIProcessingStatus.Pending, analysis.ProcessingState);
    }

    [Fact]
    public async Task TestB_PermanentFailure_RecordsFailureAndSucceedsProcessing()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var aiManager = scope.ServiceProvider.GetRequiredService<IAIWorkflowManager>();
        var fakeProvider = scope.ServiceProvider.GetRequiredService<FakeAIModelProvider>();

        var workspaceId = Guid.NewGuid();
        
        var workspace = NexusMail.Domain.Workspace.Entities.Workspace.Create("Test Workspace", Guid.NewGuid());
        var idProp = typeof(NexusMail.Shared.Domain.EntityBase<Guid>).GetProperty("Id");
        idProp?.SetValue(workspace, workspaceId);
        dbContext.Workspaces.Add(workspace);

        var accountId = Guid.NewGuid();
        var account = NexusMail.Domain.Email.Entities.EmailAccount.Create(workspaceId, NexusMail.Domain.Email.Enums.EmailProvider.Google, "test@example.com", "token", "refresh", null);
        idProp?.SetValue(account, accountId);
        dbContext.EmailAccounts.Add(account);
        var email = new NexusMail.Domain.Email.Entities.Email(accountId, workspaceId, "MessageId", "sender@example.com", "Test", "Body", DateTimeOffset.UtcNow);
        dbContext.Emails.Add(email);
        await dbContext.SaveChangesAsync();

        // Simulate permanent failure (e.g. malformed JSON that fails parsing in OpenAILanguageService)
        fakeProvider.CompletionBehavior = request => new AICompletionResponse 
        { 
            Content = "invalid json", 
            ProviderName = fakeProvider.ProviderName, 
            ModelId = fakeProvider.ModelId, 
            PromptVersion = request.PromptVersion, 
            IsSuccess = true 
        };

        // Act
        var result = await aiManager.ProcessEmailAIAsync(email.Id, workspaceId, default);

        // Assert - The consumer gets success, so it ACKs and doesn't retry
        Assert.True(result.IsSuccess);

        var analysis = await dbContext.AIAnalyses.FirstOrDefaultAsync(x => x.EmailId == email.Id);
        Assert.NotNull(analysis);
        Assert.Equal(AIProcessingStatus.Failed, analysis.ProcessingState);
        Assert.Contains("parsing failed", analysis.ProcessingError, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TestC_BodyTruncation_LimitsTo12000Characters()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var aiManager = scope.ServiceProvider.GetRequiredService<IAIWorkflowManager>();
        var fakeProvider = scope.ServiceProvider.GetRequiredService<FakeAIModelProvider>();

        var workspaceId = Guid.NewGuid();
        
        var workspace = NexusMail.Domain.Workspace.Entities.Workspace.Create("Test Workspace", Guid.NewGuid());
        var idProp = typeof(NexusMail.Shared.Domain.EntityBase<Guid>).GetProperty("Id");
        idProp?.SetValue(workspace, workspaceId);
        dbContext.Workspaces.Add(workspace);

        var accountId = Guid.NewGuid();
        var account = NexusMail.Domain.Email.Entities.EmailAccount.Create(workspaceId, NexusMail.Domain.Email.Enums.EmailProvider.Google, "test@example.com", "token", "refresh", null);
        idProp?.SetValue(account, accountId);
        dbContext.EmailAccounts.Add(account);
        var longBody = new string('A', 15000);
        var email = new NexusMail.Domain.Email.Entities.Email(accountId, workspaceId, "MessageId", "sender@example.com", "Test", longBody, DateTimeOffset.UtcNow);
        dbContext.Emails.Add(email);
        await dbContext.SaveChangesAsync();

        string capturedPromptContent = string.Empty;
        fakeProvider.CompletionBehavior = request => 
        {
            capturedPromptContent = request.Messages.First().Content;
            return new AICompletionResponse 
            { 
                Content = "{ \"classification\": { \"category\": \"Personal\", \"confidence\": 1, \"tags\": [] }, \"priority\": { \"score\": 50, \"reason\": \"\" }, \"summary\": \"\", \"needsAttention\": false }", 
                ProviderName = fakeProvider.ProviderName, 
                ModelId = fakeProvider.ModelId, 
                PromptVersion = request.PromptVersion, 
                IsSuccess = true 
            };
        };

        // Act
        await aiManager.ProcessEmailAIAsync(email.Id, workspaceId, default);

        // Assert
        Assert.Contains(new string('A', 12000), capturedPromptContent);
        Assert.DoesNotContain(new string('A', 12001), capturedPromptContent);
    }
}
