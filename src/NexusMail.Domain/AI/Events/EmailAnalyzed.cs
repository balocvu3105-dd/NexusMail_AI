using System;
using NexusMail.Shared.Events;

namespace NexusMail.Domain.AI.Events;

public sealed record EmailAnalyzed : EventBase
{
    public Guid EmailId { get; init; }
    public string Language { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public double Confidence { get; init; }
    public System.Collections.Generic.List<string> Tags { get; init; } = new();
}

