using MediatR;
using Microsoft.Extensions.Logging;
using NexusMail.Domain.Email.Events;

namespace NexusMail.EventHandlers.Email;

/// <summary>
/// Handles EmailSyncCompletedEvent — logs outcome, triggers any post-sync workflows.
/// Extend this handler to notify dashboards or update sync schedules.
/// </summary>
public sealed class EmailSyncCompletedEventHandler : INotificationHandler<EmailSyncCompletedEvent>
{
    private readonly ILogger<EmailSyncCompletedEventHandler> _logger;

    public EmailSyncCompletedEventHandler(ILogger<EmailSyncCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(EmailSyncCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Email sync completed — AccountId: {AccountId}, EmailsSynced: {Count}, OccurredAt: {OccurredAt}",
            notification.EmailAccountId,
            notification.EmailsSyncedCount,
            notification.OccurredOnUtc);

        // TODO Sprint 4: update sync state, schedule next delta sync
        return Task.CompletedTask;
    }
}
