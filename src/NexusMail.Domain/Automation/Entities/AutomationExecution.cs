using System;
using NexusMail.Shared.Domain;

namespace NexusMail.Domain.Automation.Entities;

public class AutomationExecution : AggregateRoot
{
    public Guid RuleId { get; private set; }
    public Guid EmailId { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    private AutomationExecution() { }

    public static AutomationExecution Create(Guid ruleId, Guid emailId, string status)
    {
        return new AutomationExecution
        {
            Id = Guid.NewGuid(),
            RuleId = ruleId,
            EmailId = emailId,
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void RaiseRuleMatchedEvent(NexusMail.Shared.Domain.IDomainEvent domainEvent)
    {
        AddDomainEvent(domainEvent);
    }
}
