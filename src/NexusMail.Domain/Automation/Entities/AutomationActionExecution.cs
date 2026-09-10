using System;
using NexusMail.Shared.Domain;

namespace NexusMail.Domain.Automation.Entities;

public class AutomationActionExecution : EntityBase
{
    public Guid ExecutionId { get; private set; }
    public string ActionKey { get; private set; } = string.Empty;
    public string ActionType { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    
    public int RetryCount { get; private set; }
    public string? ErrorMessage { get; private set; }

    private AutomationActionExecution() { }

    public static AutomationActionExecution Create(Guid executionId, string actionKey, string actionType)
    {
        return new AutomationActionExecution
        {
            Id = Guid.NewGuid(),
            ExecutionId = executionId,
            ActionKey = actionKey,
            ActionType = actionType,
            Status = "Pending",
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkExecuting()
    {
        Status = "Executing";
    }

    public void MarkUnknown(string reason)
    {
        Status = "Unknown";
        ErrorMessage = reason;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void MarkSuccess()
    {
        Status = "Success";
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string? errorMessage = null)
    {
        Status = "Failed";
        ErrorMessage = errorMessage;
        CompletedAt = DateTimeOffset.UtcNow;
    }
    
    public void IncrementRetry()
    {
        RetryCount++;
    }
}
