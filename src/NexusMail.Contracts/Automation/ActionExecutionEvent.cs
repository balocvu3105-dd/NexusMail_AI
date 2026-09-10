using System;

namespace NexusMail.Contracts.Automation;

/// <summary>
/// Triggered when an Automation Rule passes its conditions and its actions need to be executed.
/// Consumed by: NexusMail.Worker.Automation
/// </summary>
public sealed record ActionExecutionEvent
{
    public Guid ExecutionId { get; init; }
    public Guid RuleId { get; init; }
    public Guid EmailId { get; init; }
    public Guid WorkspaceId { get; init; }
    public string ActionsJson { get; init; } = "[]";
    public EvaluateRulesMessage EvaluateContext { get; init; } = null!;
}
