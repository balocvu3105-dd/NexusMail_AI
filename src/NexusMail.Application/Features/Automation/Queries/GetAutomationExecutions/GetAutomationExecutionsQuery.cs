using System;
using System.Collections.Generic;
using NexusMail.Shared.Common;
using NexusMail.Application.Abstractions.Messaging;

namespace NexusMail.Application.Features.Automation.Queries.GetAutomationExecutions;

public record GetAutomationExecutionsQuery(Guid WorkspaceId) : IQuery<Result<List<AutomationExecutionDto>>>;

public class AutomationExecutionDto
{
    public Guid Id { get; init; }
    public Guid RuleId { get; init; }
    public Guid EmailId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}
