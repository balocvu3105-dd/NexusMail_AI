using System;
using System.Collections.Generic;
using NexusMail.Shared.Events;

namespace NexusMail.Domain.AI.Events;

public sealed record EmailAIProcessingCompleted : EventBase
{
    public Guid EmailId { get; init; }
    
    // Status indicators
    public bool SummarySucceeded { get; init; }
    public bool PrioritySucceeded { get; init; }
    public bool ClassificationSucceeded { get; init; }
    public bool EmbeddingSucceeded { get; init; }

    // Snapshot payload (null if capability failed)
    public string? Summary { get; init; }
    public int? PriorityScore { get; init; }
    public string? Category { get; init; }
    public string? Language { get; init; }
    public List<string>? Tags { get; init; }
}
