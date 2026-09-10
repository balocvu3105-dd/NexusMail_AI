using System;
using NexusMail.Shared.Domain;
using NexusMail.Domain.Automation.Events;

namespace NexusMail.Domain.Automation.Entities;

public class Rule : AggregateRoot
{
    public Guid WorkspaceId { get; private set; }
    public string TriggerEvent { get; private set; } = string.Empty;
    public string Condition { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;

    private Rule() { }

    public Rule(Guid workspaceId, string triggerEvent, string condition, string action)
    {
        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        TriggerEvent = triggerEvent;
        Condition = condition;
        Action = action;
    }

    public void RecordExecution()
    {
        AddDomainEvent(new RuleExecuted
        {
            RuleId = Id,
            TriggerEvent = TriggerEvent,
            Action = Action,
            ExecutedAt = DateTimeOffset.UtcNow
        });
    }
}
