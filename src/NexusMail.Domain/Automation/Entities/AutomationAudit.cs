using System;
using NexusMail.Shared.Domain;

namespace NexusMail.Domain.Automation.Entities;

public class AutomationAudit : AggregateRoot
{
    public Guid RuleId { get; private set; }
    public Guid EmailId { get; private set; }
    public Guid ExecutionId { get; private set; }
    public string Executor { get; private set; } = string.Empty;
    public string Result { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; }
    public DateTime ExecutedAt { get; private set; }

    private AutomationAudit() { }

    public static AutomationAudit Create(Guid ruleId, Guid emailId, Guid executionId, string executor, string result, string? errorMessage = null)
    {
        return new AutomationAudit
        {
            Id = Guid.NewGuid(),
            RuleId = ruleId,
            EmailId = emailId,
            ExecutionId = executionId,
            Executor = executor,
            Result = result,
            ErrorMessage = errorMessage,
            ExecutedAt = DateTime.UtcNow
        };
    }
}
