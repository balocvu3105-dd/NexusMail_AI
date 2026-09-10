using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using NexusMail.Contracts.AI;
using NexusMail.Contracts.Automation;
using NexusMail.Contracts.Search;
using NexusMail.Domain.Email.Events;

namespace NexusMail.EventHandlers.Email;

/// <summary>
/// Handles the EmailReceived domain event (in-process, raised by Email Sync Pipeline).
///
/// Responsibilities:
///   1. Translate to integration messages (Contracts)
///   2. Fan out to downstream services via message bus:
///      - AI Worker: summarize, classify, embed
///      - Automation Worker: evaluate rules
///      - Search: index the email
///
/// IMPORTANT: This handler deliberately does NOT know how AI, Automation, or Search work.
/// It just publishes contracts and lets each consumer handle its own logic.
/// </summary>
public sealed class EmailReceivedDomainEventHandler : INotificationHandler<EmailReceived>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<EmailReceivedDomainEventHandler> _logger;

    public EmailReceivedDomainEventHandler(
        IPublishEndpoint publishEndpoint,
        ILogger<EmailReceivedDomainEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(EmailReceived notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "EmailReceived domain event — EmailId: {EmailId}, AccountId: {AccountId}",
            notification.EmailId, notification.AccountId);

        // 1. Fan out to AI Worker (Split into multiple jobs)
        await Task.WhenAll(
            _publishEndpoint.Publish(new SummaryRequestedMessage
            {
                EmailId = notification.EmailId,
                WorkspaceId = notification.WorkspaceId
            }, cancellationToken),
            _publishEndpoint.Publish(new PriorityRequestedMessage
            {
                EmailId = notification.EmailId,
                WorkspaceId = notification.WorkspaceId
            }, cancellationToken),
            _publishEndpoint.Publish(new ClassificationRequestedMessage
            {
                EmailId = notification.EmailId,
                WorkspaceId = notification.WorkspaceId
            }, cancellationToken),
            _publishEndpoint.Publish(new EmbeddingRequestedMessage
            {
                EmailId = notification.EmailId,
                WorkspaceId = notification.WorkspaceId
            }, cancellationToken)
        );

        // 2. Fan out to Automation Worker
        await _publishEndpoint.Publish(new EvaluateRulesMessage
        {
            EmailId = notification.EmailId,
            WorkspaceId = notification.WorkspaceId,
            Subject = notification.Subject,
            Sender = notification.Sender,
            SenderDomain = ExtractDomain(notification.Sender),
            ReceivedAt = notification.ReceivedAt,
            CorrelationId = notification.EventId.ToString()
        }, cancellationToken);

        _logger.LogDebug(
            "EmailReceived fan-out complete — AI (Split Jobs) and Automation notified for EmailId: {EmailId}",
            notification.EmailId);
    }

    private static string ExtractDomain(string sender)
    {
        var atIndex = sender.IndexOf('@');
        return atIndex >= 0 && atIndex < sender.Length - 1
            ? sender[(atIndex + 1)..]
            : string.Empty;
    }
}
