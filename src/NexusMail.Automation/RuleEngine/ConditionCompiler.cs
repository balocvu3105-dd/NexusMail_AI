using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using NexusMail.Automation.RuleEngine.AST;
using NexusMail.Contracts.Automation;

namespace NexusMail.Automation.RuleEngine;

/// <summary>
/// Compiles AST into a highly optimized Func delegate.
/// </summary>
public static class ConditionCompiler
{
    public static Func<EvaluateRulesMessage, bool> Compile(ExpressionNode node)
    {
        return node switch
        {
            LogicalExpressionNode logical => CompileLogical(logical),
            ComparisonExpressionNode comp => CompileComparison(comp),
            NotExpressionNode not => CompileNot(not),
            _ => throw new NotSupportedException($"Unsupported expression node type: {node.GetType().Name}")
        };
    }

    private static Func<EvaluateRulesMessage, bool> CompileLogical(LogicalExpressionNode node)
    {
        var funcs = node.Operands.Select(Compile).ToList();

        if (node.Operator.Equals("AND", StringComparison.OrdinalIgnoreCase))
        {
            return msg => funcs.All(f => f(msg));
        }
        else if (node.Operator.Equals("OR", StringComparison.OrdinalIgnoreCase))
        {
            return msg => funcs.Any(f => f(msg));
        }

        throw new NotSupportedException($"Unsupported logical operator: {node.Operator}");
    }

    private static Func<EvaluateRulesMessage, bool> CompileNot(NotExpressionNode node)
    {
        if (node.Operand == null) return _ => true;
        var func = Compile(node.Operand);
        return msg => !func(msg);
    }

    private static Func<EvaluateRulesMessage, bool> CompileComparison(ComparisonExpressionNode node)
    {
        // Keys normalized to lowercase to match AIMetadata dictionary keys (which are also lowercase).
        Func<EvaluateRulesMessage, object?> propertyGetter = node.Property.ToLowerInvariant() switch
        {
            "priorityscore" => msg => GetAIProperty(msg, "priorityscore"),
            "category"      => msg => GetAIProperty(msg, "category"),
            "sentiment"     => msg => GetAIProperty(msg, "sentiment"),   // reserved, populated in future steps
            "summary"       => msg => GetAIProperty(msg, "summary"),
            "tags"          => msg => GetAIProperty(msg, "tags"),
            "language"      => msg => GetAIProperty(msg, "language"),
            "subject"       => msg => msg.Subject,
            "sender"        => msg => msg.Sender,
            "senderdomain"  => msg => msg.SenderDomain,
            "hasattachments"=> msg => msg.HasAttachments,
            _ => msg => null // Ignore unknown properties — not a MissingContextException, just not relevant
        };

        var targetValue = node.Value;
        var op = node.Operator.ToUpperInvariant();

        return msg =>
        {
            var propValue = propertyGetter(msg);
            
            if (propValue == null && targetValue == null) return op == "==";
            if (propValue == null || targetValue == null) return op == "!=";

            // Simple string comparison for equality
            if (op == "==")
                return propValue.ToString()!.Equals(targetValue.ToString(), StringComparison.OrdinalIgnoreCase);
            
            if (op == "!=")
                return !propValue.ToString()!.Equals(targetValue.ToString(), StringComparison.OrdinalIgnoreCase);

            if (op == "CONTAINS")
            {
                // Handle List<string> directly (in-process path)
                if (propValue is IEnumerable<string> list)
                    return list.Contains(targetValue.ToString()!, StringComparer.OrdinalIgnoreCase);

                // Handle JsonElement array (post-serialization path, e.g. when AIMetadata is deserialized from JSON)
                if (propValue is System.Text.Json.JsonElement jsonEl && jsonEl.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var element in jsonEl.EnumerateArray())
                    {
                        if (element.ValueKind == System.Text.Json.JsonValueKind.String &&
                            element.GetString()?.Equals(targetValue.ToString(), StringComparison.OrdinalIgnoreCase) == true)
                            return true;
                    }
                    return false;
                }

                // Handle string CONTAINS (substring)
                return propValue.ToString()!.Contains(targetValue.ToString()!, StringComparison.OrdinalIgnoreCase);
            }

            // Numeric comparisons
            if (double.TryParse(propValue.ToString(), out var numProp) && double.TryParse(targetValue.ToString(), out var numTarget))
            {
                return op switch
                {
                    ">" => numProp > numTarget,
                    "<" => numProp < numTarget,
                    ">=" => numProp >= numTarget,
                    "<=" => numProp <= numTarget,
                    _ => false
                };
            }

            return false;
        };
    }

    private static object? GetAIProperty(EvaluateRulesMessage msg, string key)
    {
        if (msg.AIMetadata != null && msg.AIMetadata.TryGetValue(key, out var val))
        {
            return val;
        }
        throw new MissingContextException(key);
    }
}
