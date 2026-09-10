using NexusMail.Domain.Automation.Enums;
using NexusMail.Shared.Domain;

namespace NexusMail.Domain.Automation.Entities;

/// <summary>
/// Records a single execution of an automation rule.
/// Provides audit trail and debugging information.
/// </summary>
public sealed class RuleExecution : EntityBase
{
    public Guid RuleId { get; private set; }
    public Guid WorkspaceId { get; private set; }

    /// <summary>Source event that triggered this execution (e.g., EmailReceived).</summary>
    public TriggerType TriggerType { get; private set; }

    /// <summary>ID of the entity that triggered the rule (e.g., EmailId).</summary>
    public Guid TriggerEntityId { get; private set; }

    public bool WasSuccessful { get; private set; }
    public string? ErrorMessage { get; private set; }

    /// <summary>JSON-serialized list of actions that were executed with their outcomes.</summary>
    public string ActionsExecutedJson { get; private set; } = "[]";

    public DateTimeOffset ExecutedAt { get; private set; }
    public TimeSpan ExecutionDuration { get; private set; }

    private RuleExecution() { }

    public static RuleExecution CreateSuccess(
        Guid ruleId,
        Guid workspaceId,
        TriggerType triggerType,
        Guid triggerEntityId,
        string actionsExecutedJson,
        TimeSpan duration)
        => new()
        {
            Id = Guid.NewGuid(),
            RuleId = ruleId,
            WorkspaceId = workspaceId,
            TriggerType = triggerType,
            TriggerEntityId = triggerEntityId,
            WasSuccessful = true,
            ActionsExecutedJson = actionsExecutedJson,
            ExecutedAt = DateTimeOffset.UtcNow,
            ExecutionDuration = duration
        };

    public static RuleExecution CreateFailure(
        Guid ruleId,
        Guid workspaceId,
        TriggerType triggerType,
        Guid triggerEntityId,
        string errorMessage,
        TimeSpan duration)
        => new()
        {
            Id = Guid.NewGuid(),
            RuleId = ruleId,
            WorkspaceId = workspaceId,
            TriggerType = triggerType,
            TriggerEntityId = triggerEntityId,
            WasSuccessful = false,
            ErrorMessage = errorMessage,
            ExecutedAt = DateTimeOffset.UtcNow,
            ExecutionDuration = duration
        };
}
