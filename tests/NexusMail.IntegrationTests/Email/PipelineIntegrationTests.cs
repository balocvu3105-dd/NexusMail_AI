using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Application.Features.Email.Services;
using NexusMail.Domain.Email.Entities;
using NexusMail.Infrastructure.Email.Providers;
using Xunit;

namespace NexusMail.IntegrationTests.Email;

[Collection("IntegrationTests")]
public class PipelineIntegrationTests : IntegrationTestBase
{
    private async Task<EmailAccount> CreateTestAccountAsync()
    {
        var creatorId = Guid.NewGuid();
        var workspace = NexusMail.Domain.Workspace.Entities.Workspace.Create("Test Workspace", creatorId);
        var workspaceId = workspace.Id;
        await DbContext.Workspaces.AddAsync(workspace);
        
        var account = EmailAccount.Create(workspaceId, NexusMail.Domain.Email.Enums.EmailProvider.Google, "test@nexusmail.com", "fake_token", "fake_refresh", null);
        await DbContext.EmailAccounts.AddAsync(account);
        await DbContext.SaveChangesAsync();
        return account;
    }

    [Fact]
    public async Task InitialSync_Should_SaveEmails_And_SetHistoryId()
    {
        // Arrange
        var account = await CreateTestAccountAsync();
        var syncService = Scope.ServiceProvider.GetRequiredService<IEmailSynchronizationService>();

        // Act
        await syncService.SyncEmailAccountAsync(account.Id, Guid.NewGuid().ToString());

        // Assert
        var updatedAccount = await DbContext.EmailAccounts.Include(a => a.State).FirstAsync(a => a.Id == account.Id);
        updatedAccount.State.Status.Should().Be(NexusMail.Domain.Email.Enums.EmailSyncStatus.Idle);
        updatedAccount.State.ProviderCursor.Should().NotBeNull();
        
        // Using FakeEmailProvider, it generates a HistoryId "H100" by default
        updatedAccount.State.ProviderCursor.Should().Be("H100");

        var emailsCount = await DbContext.Emails.CountAsync(e => e.AccountId == account.Id);
        emailsCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task IncrementalSync_Should_UseProviderCursor_And_SaveNewEmails()
    {
        // Arrange
        var account = await CreateTestAccountAsync();
        var syncService = Scope.ServiceProvider.GetRequiredService<IEmailSynchronizationService>();

        // Set initial state
        account.State.MarkAttemptStarted(Guid.NewGuid().ToString());
        account.State.MarkSuccess(50, "H50");
        await DbContext.SaveChangesAsync();

        // Act - this sync should use "H50"
        await syncService.SyncEmailAccountAsync(account.Id, Guid.NewGuid().ToString());

        // Assert
        var updatedAccount = await DbContext.EmailAccounts.Include(a => a.State).FirstAsync(a => a.Id == account.Id);
        updatedAccount.State.ProviderCursor.Should().Be("H100"); // FakeEmailProvider always returns H100
        
        var emailsCount = await DbContext.Emails.CountAsync(e => e.AccountId == account.Id);
        emailsCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Sync_Should_Deduplicate_BasedOn_MessageId()
    {
        // Arrange
        var account = await CreateTestAccountAsync();
        var syncService = Scope.ServiceProvider.GetRequiredService<IEmailSynchronizationService>();

        // Act - run sync twice
        await syncService.SyncEmailAccountAsync(account.Id, Guid.NewGuid().ToString());
        var firstRunCount = await DbContext.Emails.CountAsync(e => e.AccountId == account.Id);
        
        // Run again
        await syncService.SyncEmailAccountAsync(account.Id, Guid.NewGuid().ToString());
        var secondRunCount = await DbContext.Emails.CountAsync(e => e.AccountId == account.Id);

        // Assert - count should not increase because FakeEmailProvider returns the same messages
        firstRunCount.Should().BeGreaterThan(0);
        secondRunCount.Should().Be(firstRunCount);
    }

    [Fact]
    public async Task Sync_Should_PersistEmail_And_PublishEvents_In_Same_Transaction()
    {
        // Arrange
        var account = await CreateTestAccountAsync();
        var syncService = Scope.ServiceProvider.GetRequiredService<IEmailSynchronizationService>();
        var harness = Scope.ServiceProvider.GetRequiredService<MassTransit.Testing.ITestHarness>();
        await harness.Start();

        // Act
        await syncService.SyncEmailAccountAsync(account.Id, Guid.NewGuid().ToString());

        // Assert
        var emails = await DbContext.Emails.Where(e => e.AccountId == account.Id).ToListAsync();
        emails.Should().NotBeEmpty();
        
        var outboxCount = await DbContext.Database.SqlQueryRaw<int>("SELECT COUNT(*) as \"Value\" FROM \"OutboxMessages\"").FirstOrDefaultAsync();
        outboxCount.Should().BeGreaterThan(0, "Outbox messages should be created by the interceptor");

        // Manually run OutboxDispatcher to publish domain events to MediatR which then publishes to MassTransit
        var dispatcher = Scope.ServiceProvider.GetRequiredService<NexusMail.Application.Abstractions.Outbox.IOutboxDispatcher>();
        await dispatcher.DispatchUnprocessedMessagesAsync();

        // Verify that the domain event handler published messages to the bus
        var hasPublished = await harness.Published.Any<NexusMail.Contracts.AI.AIProcessingRequestedMessage>();
        hasPublished.Should().BeTrue("Expected downstream contract messages to be published");
    }

    [Fact]
    public async Task MultipleAccounts_Should_Be_Isolated()
    {
        // Arrange
        var account1 = await CreateTestAccountAsync();
        var account2 = await CreateTestAccountAsync();
        var syncService = Scope.ServiceProvider.GetRequiredService<IEmailSynchronizationService>();

        // Act
        await syncService.SyncEmailAccountAsync(account1.Id, Guid.NewGuid().ToString());
        await syncService.SyncEmailAccountAsync(account2.Id, Guid.NewGuid().ToString());

        // Assert
        var emails1 = await DbContext.Emails.CountAsync(e => e.AccountId == account1.Id);
        var emails2 = await DbContext.Emails.CountAsync(e => e.AccountId == account2.Id);

        emails1.Should().BeGreaterThan(0);
        emails2.Should().Be(emails1); // Both sync the same fake provider but create separate email records
    }
}
