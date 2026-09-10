using System;

namespace NexusMail.Domain.Automation.Entities;

public class AutomationRuleMetric
{
    public Guid Id { get; private set; }
    public Guid RuleId { get; private set; }
    public int RuleVersion { get; private set; }
    public Guid EmailId { get; private set; }
    public string Result { get; private set; } = string.Empty; // Evaluated, Matched, Success, PermanentFailure, Skipped, Retry
    public double ExecutionLatencyMs { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public string? ErrorMessage { get; private set; }

    private AutomationRuleMetric() { }

    public static AutomationRuleMetric Create(
        Guid ruleId,
        int ruleVersion,
        Guid emailId,
        string result,
        double executionLatencyMs,
        string? errorMessage = null)
    {
        return new AutomationRuleMetric
        {
            Id = Guid.NewGuid(),
            RuleId = ruleId,
            RuleVersion = ruleVersion,
            EmailId = emailId,
            Result = result,
            ExecutionLatencyMs = executionLatencyMs,
            Timestamp = DateTimeOffset.UtcNow,
            ErrorMessage = errorMessage
        };
    }
}
