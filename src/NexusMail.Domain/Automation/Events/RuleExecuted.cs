using System;
using NexusMail.Shared.Events;

namespace NexusMail.Domain.Automation.Events;

public sealed record RuleExecuted : EventBase
{
    public Guid RuleId { get; init; }
    public string TriggerEvent { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public DateTimeOffset ExecutedAt { get; init; }
}

