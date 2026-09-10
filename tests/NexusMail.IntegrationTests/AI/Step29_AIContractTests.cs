using System;
using System.Collections.Generic;
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
using MediatR;
using NexusMail.Shared.Domain;
using NexusMail.Shared.Common;
using NexusMail.Application.Features.Email.Abstractions;

namespace NexusMail.IntegrationTests.AI;

public class Step29_AIContractTests : IntegrationTestBase
{
    private readonly Mock<ISummaryService> _summaryServiceMock;
    private readonly Mock<IClassificationService> _classificationServiceMock;

    public Step29_AIContractTests()
    {
        _summaryServiceMock = new Mock<ISummaryService>();
        _classificationServiceMock = new Mock<IClassificationService>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        // Replace real AI services with Mocks
        services.AddScoped<ISummaryService>(_ => _summaryServiceMock.Object);
        services.AddScoped<IClassificationService>(_ => _classificationServiceMock.Object);
    }

    [Fact]
    public async Task TransientProviderFailure_ShouldRemainPendingAndThrow()
    {
        // Arrange
        var mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        var emailRepo = Scope.ServiceProvider.GetRequiredService<IEmailRepository>();
        var dbContext = DbContext;
        var workspace = NexusMail.Domain.Workspace.Entities.Workspace.Create("Test Workspace", Guid.NewGuid());
        var workspaceId = workspace.Id;
        dbContext.Workspaces.Add(workspace);
        var account = NexusMail.Domain.Email.Entities.EmailAccount.Create(workspaceId, NexusMail.Domain.Email.Enums.EmailProvider.Google, "test@test.com", "valid", "refresh", DateTime.UtcNow.AddHours(1));
        var accountId = account.Id;
        dbContext.EmailAccounts.Add(account);
        var email = new NexusMail.Domain.Email.Entities.Email(accountId, workspaceId, "msg1", "sender@test.com", "Subject", "Body", DateTimeOffset.UtcNow);
        dbContext.Emails.Add(email);
        await dbContext.SaveChangesAsync();

        _summaryServiceMock
            .Setup(x => x.GenerateSummaryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AIProviderTransientException("Rate limit 429"));

        // Act
        var command = new GenerateSummaryCommand(email.Id, workspaceId);
        
        var act = async () => await mediator.Send(command);

        // Assert
        await act.Should().ThrowAsync<AIProviderTransientException>();

        // Verify state is Pending
        var savedAnalysis = await dbContext.Set<AIAnalysis>().FirstOrDefaultAsync(a => a.EmailId == email.Id);
        savedAnalysis.Should().NotBeNull();
        savedAnalysis!.SummaryStatus.Should().Be(AIProcessingStatus.Pending);
    }

    [Fact]
    public async Task PermanentProviderFailure_ShouldPersistFailedAndNotThrow()
    {
        // Arrange
        var mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        var emailRepo = Scope.ServiceProvider.GetRequiredService<IEmailRepository>();
        var dbContext = DbContext;
        var workspace = NexusMail.Domain.Workspace.Entities.Workspace.Create("Test Workspace", Guid.NewGuid());
        var workspaceId = workspace.Id;
        dbContext.Workspaces.Add(workspace);
        var account = NexusMail.Domain.Email.Entities.EmailAccount.Create(workspaceId, NexusMail.Domain.Email.Enums.EmailProvider.Google, "test2@test.com", "valid", "refresh", DateTime.UtcNow.AddHours(1));
        var accountId = account.Id;
        dbContext.EmailAccounts.Add(account);
        var email = new NexusMail.Domain.Email.Entities.Email(accountId, workspaceId, "msg2", "sender@test.com", "Subject", "Body", DateTimeOffset.UtcNow);
        dbContext.Emails.Add(email);
        await dbContext.SaveChangesAsync();

        _summaryServiceMock
            .Setup(x => x.GenerateSummaryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<string>(new Error("AI.Permanent", "Failed")));

        // Act
        var command = new GenerateSummaryCommand(email.Id, workspaceId);
        var result = await mediator.Send(command); // Should NOT throw

        // Assert
        if (!result.IsSuccess)
        {
            throw new Exception($"RESULT ERROR: {result.Error?.Code} - {result.Error?.Description}");
        }
        result.IsSuccess.Should().BeTrue(); // Handler completes successfully, though the analysis itself failed

        var savedAnalysis = await dbContext.Set<AIAnalysis>().FirstOrDefaultAsync(a => a.EmailId == email.Id);
        savedAnalysis.Should().NotBeNull();
        savedAnalysis!.SummaryStatus.Should().Be(AIProcessingStatus.Failed);
        savedAnalysis.SummaryError.Should().Be("Failed");
    }

    [Fact]
    public async Task DuplicateSummaryRequest_ShouldNotRegenerate()
    {
        // Arrange
        var mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        var emailRepo = Scope.ServiceProvider.GetRequiredService<IEmailRepository>();
        var dbContext = DbContext;
        var workspace = NexusMail.Domain.Workspace.Entities.Workspace.Create("Test Workspace", Guid.NewGuid());
        var workspaceId = workspace.Id;
        dbContext.Workspaces.Add(workspace);
        var account = NexusMail.Domain.Email.Entities.EmailAccount.Create(workspaceId, NexusMail.Domain.Email.Enums.EmailProvider.Google, "test3@test.com", "valid", "refresh", DateTime.UtcNow.AddHours(1));
        var accountId = account.Id;
        dbContext.EmailAccounts.Add(account);
        var email = new NexusMail.Domain.Email.Entities.Email(accountId, workspaceId, "msg3", "sender@test.com", "Subject", "Body", DateTimeOffset.UtcNow);
        dbContext.Emails.Add(email);

        var analysis = AIAnalysis.Create(email.Id);
        analysis.StartSummary(Guid.NewGuid());
        analysis.CompleteSummary(analysis.SummaryAttemptId!.Value, "First Summary");
        dbContext.Set<AIAnalysis>().Add(analysis);
        
        await dbContext.SaveChangesAsync();

        _summaryServiceMock
            .Setup(x => x.GenerateSummaryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("Second Summary"));

        // Act
        var command = new GenerateSummaryCommand(email.Id, workspaceId);
        var result = await mediator.Send(command);

        // Assert
        if (!result.IsSuccess)
        {
            var dbEmail = await dbContext.Emails.FirstOrDefaultAsync(e => e.Id == email.Id);
            var dbAccount = await dbContext.EmailAccounts.FirstOrDefaultAsync(a => a.Id == account.Id);
            var joinResult = await (from e in dbContext.Emails
                                    join a in dbContext.EmailAccounts on e.AccountId equals a.Id
                                    where e.Id == email.Id
                                    select new { e.Id, a.WorkspaceId }).FirstOrDefaultAsync();
            throw new Exception($"RESULT ERROR: {result.Error?.Code} - {result.Error?.Description} | DB Email: {dbEmail?.Id} Acct: {dbEmail?.AccountId}, DB Account: {dbAccount?.Id} WS: {dbAccount?.WorkspaceId}, JoinResult: {joinResult?.Id}, {joinResult?.WorkspaceId} - EXPECTED: e.Id={email.Id}, a.WS={workspaceId}");
        }
        result.IsSuccess.Should().BeTrue();
        
        // Ensure service was never called because it was already Succeeded
        _summaryServiceMock.Verify(x => x.GenerateSummaryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        var savedAnalysis = await dbContext.Set<AIAnalysis>().FirstOrDefaultAsync(a => a.EmailId == email.Id);
        savedAnalysis!.Summary.Should().Be("First Summary"); // Retained original
    }
}
