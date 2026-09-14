using NexusMail.Domain.Automation.Enums;
using NexusMail.Domain.Automation.Events;
using NexusMail.Shared.Domain;

namespace NexusMail.Domain.Automation.Entities;

/// <summary>
/// An automation rule that evaluates triggers and executes actions.
///
/// IMPORTANT: AutomationRule is INDEPENDENT from the Email domain.
/// It does not reference EmailAccount or Email entities.
/// It receives data via EvaluateRulesMessage (Contracts) through the Automation Worker.
///
/// Design:
///   TriggerType → Conditions (JSON) → Actions (JSON)
///
/// Example rule:
///   IF EmailReceived AND subject contains "Invoice"
///   THEN ApplyLabel("Invoice") AND SendNotification AND CallWebhook
/// </summary>
public sealed class AutomationRule : AggregateRoot
{
    public Guid WorkspaceId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TriggerType TriggerType { get; private set; }

    /// <summary>JSON-serialized condition tree. Evaluated by RuleEngine.</summary>
    public string ConditionsJson { get; private set; } = "[]";

    /// <summary>JSON-serialized list of actions with their parameters.</summary>
    public string ActionsJson { get; private set; } = "[]";

    public bool IsEnabled { get; private set; } = true;
    public bool DryRun { get; private set; } = false;
    public int RuleVersion { get; private set; } = 1;
    public int ExecutionCount { get; private set; }
    public DateTimeOffset? LastExecutedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <summary>
    /// Derived read-only metadata indicating that this rule references at least one AI context property
    /// (category, priorityscore, summary, tags, language, sentiment).
    ///
    /// IMPORTANT: This property does NOT control evaluation behavior.
    /// When AI context is absent, the authoritative mechanism is:
    ///   GetAIProperty → MissingContextException → RuleEvaluator → NotApplicable
    /// RequiresAI is only informational — useful for UI hints, logging, and analytics.
    /// </summary>
    public bool RequiresAI => ConditionsJson.Contains("category", StringComparison.OrdinalIgnoreCase)
                           || ConditionsJson.Contains("priorityscore", StringComparison.OrdinalIgnoreCase)
                           || ConditionsJson.Contains("summary", StringComparison.OrdinalIgnoreCase)
                           || ConditionsJson.Contains("tags", StringComparison.OrdinalIgnoreCase)
                           || ConditionsJson.Contains("language", StringComparison.OrdinalIgnoreCase)
                           || ConditionsJson.Contains("sentiment", StringComparison.OrdinalIgnoreCase);

    private AutomationRule() { }

    public static AutomationRule Create(
        Guid workspaceId,
        string name,
        TriggerType triggerType,
        string conditionsJson = "[]",
        string actionsJson = "[]",
        string? description = null,
        bool dryRun = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var rule = new AutomationRule
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Name = name,
            Description = description,
            TriggerType = triggerType,
            ConditionsJson = conditionsJson,
            ActionsJson = actionsJson,
            IsEnabled = true,
            DryRun = dryRun,
            RuleVersion = 1,
            ExecutionCount = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };

        return rule;
    }

    public void RecordExecution()
    {
        ExecutionCount++;
        LastExecutedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new RuleTriggeredEvent(Id, WorkspaceId));
    }

    public void Enable()
    {
        IsEnabled = true;
        RuleVersion++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Disable()
    {
        IsEnabled = false;
        RuleVersion++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string name, string? description, string conditionsJson, string actionsJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Description = description;
        ConditionsJson = conditionsJson;
        ActionsJson = actionsJson;
        RuleVersion++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
