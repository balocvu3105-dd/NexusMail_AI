using System;
using System.Collections.Generic;
using NexusMail.Shared.Common;
using NexusMail.Application.Abstractions.Messaging;

namespace NexusMail.Application.Features.Automation.Queries.GetAutomationRules;

public record GetAutomationRulesQuery(Guid WorkspaceId) : IQuery<Result<List<AutomationRuleDto>>>;

public class AutomationRuleDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string TriggerType { get; init; } = string.Empty;
    public string ConditionsJson { get; init; } = "[]";
    public string ActionsJson { get; init; } = "[]";
    public bool IsEnabled { get; init; }
    public bool DryRun { get; init; }
    public int ExecutionCount { get; init; }
    public DateTimeOffset? LastExecutedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
