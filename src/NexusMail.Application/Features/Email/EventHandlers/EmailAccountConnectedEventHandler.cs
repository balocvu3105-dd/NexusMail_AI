using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Domain.Email.Events;

namespace NexusMail.Application.Features.Email.EventHandlers;

public sealed class EmailAccountConnectedEventHandler : INotificationHandler<EmailAccountConnectedEvent>
{
    private readonly IEmailSynchronizationService _syncService;
    private readonly ILogger<EmailAccountConnectedEventHandler> _logger;
    private static readonly ConcurrentDictionary<string, bool> _processedEvents = new();

    public EmailAccountConnectedEventHandler(
        IEmailSynchronizationService syncService,
        ILogger<EmailAccountConnectedEventHandler> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    public async Task Handle(EmailAccountConnectedEvent notification, CancellationToken cancellationToken)
    {
        var idempotencyKey = $"sync-init-{notification.EventId}";
        
        // Check idempotency (mock distributed cache using ConcurrentDictionary)
        if (!_processedEvents.TryAdd(idempotencyKey, true))
        {
            _logger.LogInformation("Event {EventId} already processed. Skipping.", notification.EventId);
            return;
        }

        _logger.LogInformation("Handling EmailAccountConnectedEvent for AccountId: {AccountId}", notification.EmailAccountId);

        // Start synchronization
        await _syncService.SyncEmailAccountAsync(notification.EmailAccountId, notification.EventId.ToString(), cancellationToken);
    }
}

