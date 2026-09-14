using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using NexusMail.Contracts.AI;
using NexusMail.Domain.AI.Events;
using NexusMail.Application.Abstractions.Authentication;

namespace NexusMail.EventHandlers.AI;

public sealed class EmailAIProcessingCompletedDomainEventHandler : INotificationHandler<EmailAIProcessingCompleted>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<EmailAIProcessingCompletedDomainEventHandler> _logger;
    private readonly IWorkspaceContext _workspaceContext;

    public EmailAIProcessingCompletedDomainEventHandler(
        IPublishEndpoint publishEndpoint,
        ILogger<EmailAIProcessingCompletedDomainEventHandler> logger,
        IWorkspaceContext workspaceContext)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _workspaceContext = workspaceContext;
    }

    public async Task Handle(EmailAIProcessingCompleted notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("EmailAIProcessingCompleted event handling for EmailId: {EmailId}", notification.EmailId);

        // We use the WorkspaceId from context. In an outbox scenario this might be preserved by the Outbox,
        // or we might need to store WorkspaceId in AIAnalysis (or pass it through the Domain Event).
        // Wait, does AIAnalysis have WorkspaceId? No, it doesn't.
        // The event EmailAIProcessingCompleted should have it if we can.
        // For now we get it from WorkspaceContext (since commands run within a workspace context).
        var workspaceId = _workspaceContext.WorkspaceId ?? System.Guid.Empty;

        var integrationMessage = new AIProcessingCompletedMessage
        {
            EmailId = notification.EmailId,
            WorkspaceId = workspaceId,
            SummaryResult = new CapabilityResult<string> { Succeeded = notification.SummarySucceeded, Value = notification.Summary },
            PriorityResult = new CapabilityResult<int?> { Succeeded = notification.PrioritySucceeded, Value = notification.PrioritySucceeded ? notification.PriorityScore : null },
            ClassificationResult = new CapabilityResult<ClassificationPayload>
            {
                Succeeded = notification.ClassificationSucceeded,
                Value = notification.ClassificationSucceeded ? new ClassificationPayload { Category = notification.Category ?? string.Empty, Language = notification.Language ?? string.Empty, Tags = notification.Tags ?? new System.Collections.Generic.List<string>() } : null
            },
            EmbeddingResult = new CapabilityResult<float[]> { Succeeded = notification.EmbeddingSucceeded, Value = notification.EmbeddingVector }
        };

        await _publishEndpoint.Publish(integrationMessage, cancellationToken);
    }
}
