namespace NexusMail.Contracts.AI;

/// <summary>
/// Triggered when an email needs categorization.
/// Consumed by: NexusMail.Worker.AI
/// </summary>
public sealed record ClassificationRequestedMessage
{
    public Guid EmailId { get; init; }
    public Guid WorkspaceId { get; init; }
}
