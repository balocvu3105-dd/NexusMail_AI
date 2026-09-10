using NexusMail.Shared.Events;

namespace NexusMail.Domain.Automation.Events;

/// <summary>
/// Raised when an automation rule is triggered and executed.
/// Consumed by EventHandlers to log, notify, or trigger downstream actions.
/// </summary>
public sealed record RuleTriggeredEvent : EventBase
{
    public Guid RuleId { get; init; }
    public Guid WorkspaceId { get; init; }

    public RuleTriggeredEvent(Guid ruleId, Guid workspaceId)
    {
        RuleId = ruleId;
        WorkspaceId = workspaceId;
    }
}
