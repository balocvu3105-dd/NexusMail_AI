using System;
using System.Collections.Generic;
using NexusMail.Shared.Common;
using NexusMail.Application.Abstractions.Messaging;
using NexusMail.Application.Features.Automation.Queries.GetAutomationExecutions;

namespace NexusMail.Application.Features.Automation.Queries.GetAutomationExecutionById;

public record GetAutomationExecutionByIdQuery(Guid ExecutionId, Guid WorkspaceId) : IQuery<Result<AutomationExecutionDetailDto>>;

public class AutomationExecutionDetailDto
{
    public Guid Id { get; init; }
    public Guid RuleId { get; init; }
    public Guid EmailId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }

    public List<ActionExecutionDto> ActionExecutions { get; init; } = new();
    public List<AuditDto> Audits { get; init; } = new();
}

public class ActionExecutionDto
{
    public string ActionKey { get; init; } = string.Empty;
    public string ActionType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public int RetryCount { get; init; }
    public string? ErrorMessage { get; init; }
}

public class AuditDto
{
    public string Executor { get; init; } = string.Empty;
    public string Result { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public DateTime ExecutedAt { get; init; }
}
