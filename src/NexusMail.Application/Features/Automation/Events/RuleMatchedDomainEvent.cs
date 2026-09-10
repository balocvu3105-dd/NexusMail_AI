using System;
using NexusMail.Contracts.Automation;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.Automation.Events;

public sealed record RuleMatchedDomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOnUtc { get; } = DateTimeOffset.UtcNow;
    
    public Guid ExecutionId { get; init; }
    public Guid RuleId { get; init; }
    public Guid EmailId { get; init; }
    public Guid WorkspaceId { get; init; }
    public string ActionsJson { get; init; } = "[]";
    public EvaluateRulesMessage EvaluateContext { get; init; } = null!;
}
