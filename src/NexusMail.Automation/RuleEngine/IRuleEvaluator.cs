using System;
using System.Collections.Concurrent;
using System.Text.Json;
using NexusMail.Contracts.Automation;
using NexusMail.Automation.RuleEngine.AST;

namespace NexusMail.Automation.RuleEngine;

/// <summary>
/// Evaluates automation rule conditions against incoming trigger data.
///
/// IMPORTANT: The Rule Engine is independent from Email, AI, and Search.
/// It only knows about condition definitions (JSON) and trigger data (EvaluateRulesMessage).
///
/// Sprint 7 implementation plan:
///   - Parse ConditionsJson into a condition tree
///   - Support operators: contains, equals, starts_with, regex, greater_than
///   - Support logical: AND, OR, NOT
///   - Cache compiled rules to avoid re-parsing JSON on every evaluation
/// </summary>
public interface IRuleEvaluator
{
    /// <summary>
    /// Evaluates whether the rule conditions match the trigger data.
    /// Returns Match if all conditions pass, NoMatch if they fail, and NotApplicable if context is missing.
    /// </summary>
    RuleEvaluationResult Evaluate(string conditionsJson, EvaluateRulesMessage triggerData);
}



/// <summary>
/// Compiles JSON rules into AST, compiles AST into Delegates, and caches them.
/// </summary>
public sealed class RuleEvaluator : IRuleEvaluator
{
    private readonly ConcurrentDictionary<string, Func<EvaluateRulesMessage, bool>> _cache = new();

    public RuleEvaluationResult Evaluate(string conditionsJson, EvaluateRulesMessage triggerData)
    {
        if (string.IsNullOrWhiteSpace(conditionsJson) || conditionsJson == "[]" || conditionsJson == "{}")
        {
            return RuleEvaluationResult.Match; // No conditions = always match
        }

        Func<EvaluateRulesMessage, bool> func;
        try
        {
            func = _cache.GetOrAdd(conditionsJson, json =>
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var rootNode = JsonSerializer.Deserialize<ExpressionNode>(json, options);
                
                if (rootNode == null) return _ => false;

                return ConditionCompiler.Compile(rootNode);
            });
        }
        catch (Exception)
        {
            return RuleEvaluationResult.NoMatch;
        }

        try
        {
            return func(triggerData) ? RuleEvaluationResult.Match : RuleEvaluationResult.NoMatch;
        }
        catch (MissingContextException)
        {
            return RuleEvaluationResult.NotApplicable;
        }
        catch (Exception)
        {
            return RuleEvaluationResult.NoMatch;
        }
    }
}
