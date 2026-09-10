namespace NexusMail.Contracts.AI;

/// <summary>
/// Triggered when an email needs semantic embedding.
/// Consumed by: NexusMail.Worker.AI
/// </summary>
public sealed record EmbeddingRequestedMessage
{
    public Guid EmailId { get; init; }
    public Guid WorkspaceId { get; init; }
}
