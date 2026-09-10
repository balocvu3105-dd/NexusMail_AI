using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NexusMail.Automation.RuleEngine.AST;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(LogicalExpressionNode), typeDiscriminator: "logical")]
[JsonDerivedType(typeof(ComparisonExpressionNode), typeDiscriminator: "comparison")]
[JsonDerivedType(typeof(NotExpressionNode), typeDiscriminator: "not")]
public abstract class ExpressionNode
{
}

public class LogicalExpressionNode : ExpressionNode
{
    [JsonPropertyName("operator")]
    public string Operator { get; set; } = "AND"; // AND, OR

    [JsonPropertyName("operands")]
    public List<ExpressionNode> Operands { get; set; } = new();
}

public class ComparisonExpressionNode : ExpressionNode
{
    [JsonPropertyName("property")]
    public string Property { get; set; } = string.Empty;

    [JsonPropertyName("operator")]
    public string Operator { get; set; } = "=="; // ==, !=, >, <, >=, <=, CONTAINS, IN

    [JsonPropertyName("value")]
    public object? Value { get; set; }
}

public class NotExpressionNode : ExpressionNode
{
    [JsonPropertyName("operand")]
    public ExpressionNode? Operand { get; set; }
}
