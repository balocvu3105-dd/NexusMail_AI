using System;
using NexusMail.Shared.Domain;

namespace NexusMail.Domain.Automation.Entities;

public class AutomationExecution : AggregateRoot
{
    public Guid RuleId { get; private set; }
    public Guid EmailId { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    public int ExecutionVersion { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    private AutomationExecution() { }

    public static AutomationExecution Create(Guid ruleId, Guid emailId, string status)
    {
        return new AutomationExecution
        {
            Id = Guid.NewGuid(),
            RuleId = ruleId,
            EmailId = emailId,
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow,
            ExecutionVersion = 1
        };
    }

    public void RaiseRuleMatchedEvent(NexusMail.Shared.Domain.IDomainEvent domainEvent)
    {
        AddDomainEvent(domainEvent);
    }

    public bool Complete(string finalStatus, string? errorMessage = null)
    {
        // Check if already terminal (idempotent finalization)
        if (Status == "Succeeded" || Status == "Failed" || Status == "Unknown")
        {
            return false;
        }

        Status = finalStatus;
        
        if (finalStatus == "Succeeded" || finalStatus == "Failed" || finalStatus == "Unknown")
        {
            CompletedAt = DateTimeOffset.UtcNow;
            ErrorMessage = errorMessage;
        }
        
        ExecutionVersion++;
        return true;
    }
}
