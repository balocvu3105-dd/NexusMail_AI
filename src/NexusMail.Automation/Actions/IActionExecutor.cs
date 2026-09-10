using NexusMail.Contracts.Automation;
using NexusMail.Domain.Automation.Enums;

namespace NexusMail.Automation.Actions;

/// <summary>
/// Executes a single action from an automation rule.
///
/// Sprint 7 implementation plan:
///   - Parse ActionsJson into list of action definitions
///   - Route to appropriate executor (LabelExecutor, NotifyExecutor, WebhookExecutor...)
///   - Each executor is independent and injectable
///   - Record result (success/failure) for RuleExecution audit log
/// </summary>
public interface IActionExecutor
{
    /// <summary>The action type this executor handles.</summary>
    ActionType ActionType { get; }

    /// <summary>
    /// Execute the action with the given parameters JSON.
    /// Should be idempotent — may be retried on failure.
    /// </summary>
    Task<ActionResult> ExecuteAsync(string parametersJson, EvaluateRulesMessage context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Factory that resolves the correct IActionExecutor for a given ActionType.
/// Sprint 7: add new actions by implementing IActionExecutor and registering it here.
/// </summary>
public sealed class ActionExecutorFactory
{
    private readonly IReadOnlyDictionary<ActionType, IActionExecutor> _executors;

    public ActionExecutorFactory(IEnumerable<IActionExecutor> executors)
    {
        _executors = executors.ToDictionary(e => e.ActionType);
    }

    public IActionExecutor? GetExecutor(ActionType actionType)
        => _executors.TryGetValue(actionType, out var executor) ? executor : null;

    public bool IsSupported(ActionType actionType) => _executors.ContainsKey(actionType);
}
