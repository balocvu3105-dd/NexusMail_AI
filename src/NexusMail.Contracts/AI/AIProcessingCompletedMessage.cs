namespace NexusMail.Contracts.AI;

/// <summary>
/// Triggered when all AI processing (or partial processing) is completed.
/// Consumed by: NexusMail.Worker.Search (to index embedding), NexusMail.Worker.Automation (to run rules).
/// </summary>
public sealed record AIProcessingCompletedMessage
{
    public Guid EmailId { get; init; }
    public Guid WorkspaceId { get; init; }
    
    // Capability Results (Status + Optional Value)
    public CapabilityResult<string>? SummaryResult { get; init; }
    public CapabilityResult<int?>? PriorityResult { get; init; }
    public CapabilityResult<ClassificationPayload>? ClassificationResult { get; init; }
    public CapabilityResult<float[]>? EmbeddingResult { get; init; }
}

public sealed record CapabilityResult<T>
{
    public bool Succeeded { get; init; }
    public T? Value { get; init; }
}

public sealed record ClassificationPayload
{
    public string Category { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}
