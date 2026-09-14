using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using NexusMail.Contracts.AI;
using NexusMail.Contracts.Automation;
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
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailReceivedDomainEventHandler> _logger;

    public EmailReceivedDomainEventHandler(
        IPublishEndpoint publishEndpoint,
        IConfiguration configuration,
        ILogger<EmailReceivedDomainEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Handle(EmailReceived notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "EmailReceived domain event — EmailId: {EmailId}, AccountId: {AccountId}",
            notification.EmailId, notification.AccountId);

        // Check Processing Gate
        var cutoffString = _configuration["AI:ProcessingGateCutoffDate"];
        DateTimeOffset cutoffDate = DateTimeOffset.MinValue;
        if (!string.IsNullOrWhiteSpace(cutoffString) && DateTimeOffset.TryParse(cutoffString, out var parsed))
        {
            cutoffDate = parsed.ToUniversalTime();
        }

        bool isLiveEmail = notification.ReceivedAt >= cutoffDate;

        if (isLiveEmail)
        {
            // 1. Fan out to AI Worker (One-call architecture + Embedding)
            await Task.WhenAll(
                _publishEndpoint.Publish(new AIProcessingRequestedMessage
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
        }
        else
        {
            _logger.LogInformation("Skipping AI processing for EmailId: {EmailId}. ReceivedAt {ReceivedAt} is before cutoff {CutoffDate}", notification.EmailId, notification.ReceivedAt, cutoffDate);
        }

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
