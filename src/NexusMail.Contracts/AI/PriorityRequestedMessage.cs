namespace NexusMail.Contracts.AI;

/// <summary>
/// Triggered when an email needs priority scoring.
/// Consumed by: NexusMail.Worker.AI
/// </summary>
public sealed record PriorityRequestedMessage
{
    public Guid EmailId { get; init; }
    public Guid WorkspaceId { get; init; }
}
