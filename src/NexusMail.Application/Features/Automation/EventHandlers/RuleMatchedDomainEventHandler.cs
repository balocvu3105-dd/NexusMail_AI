using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MediatR;
using NexusMail.Contracts.Automation;
using NexusMail.Application.Features.Automation.Events;

namespace NexusMail.Application.Features.Automation.EventHandlers;

public sealed class RuleMatchedDomainEventHandler : INotificationHandler<RuleMatchedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public RuleMatchedDomainEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(RuleMatchedDomainEvent notification, CancellationToken cancellationToken)
    {
        var actionEvent = new ActionExecutionEvent
        {
            ExecutionId = notification.ExecutionId,
            RuleId = notification.RuleId,
            EmailId = notification.EmailId,
            WorkspaceId = notification.WorkspaceId,
            ActionsJson = notification.ActionsJson,
            EvaluateContext = notification.EvaluateContext
        };

        await _publishEndpoint.Publish(actionEvent, cancellationToken);
    }
}
