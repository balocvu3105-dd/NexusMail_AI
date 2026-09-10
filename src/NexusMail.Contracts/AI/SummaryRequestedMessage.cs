namespace NexusMail.Contracts.AI;

/// <summary>
/// Triggered when an email needs summarization.
/// Consumed by: NexusMail.Worker.AI
/// </summary>
public sealed record SummaryRequestedMessage
{
    public Guid EmailId { get; init; }
    public Guid WorkspaceId { get; init; }
}
