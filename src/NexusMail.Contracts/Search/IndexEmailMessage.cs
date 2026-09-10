namespace NexusMail.Contracts.Search;

/// <summary>
/// Message published to request indexing of an email into the Search engine.
/// Consumed by: NexusMail.Search indexer worker.
/// NOTE: Search does NOT reference Application — it only consumes these contracts.
/// </summary>
public sealed record IndexEmailMessage
{
    public Guid EmailId { get; init; }
    public Guid WorkspaceId { get; init; }
    public Guid EmailAccountId { get; init; }
    public string MessageId { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string Sender { get; init; } = string.Empty;
    public string SenderName { get; init; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; init; }
    public IReadOnlyList<string> Labels { get; init; } = [];
    public bool HasAttachments { get; init; }

    /// <summary>Embedding vector — populated by AI Worker before publishing to Search.</summary>
    public float[]? EmbeddingVector { get; init; }
}

/// <summary>
/// Published when a previously indexed email should be removed from the index.
/// </summary>
public sealed record RemoveEmailFromIndexMessage
{
    public Guid EmailId { get; init; }
    public Guid WorkspaceId { get; init; }
}
