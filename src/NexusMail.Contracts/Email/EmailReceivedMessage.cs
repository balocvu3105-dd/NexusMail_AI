namespace NexusMail.Contracts.Email;

/// <summary>
/// Integration message published after a new email is persisted and ready for downstream processing.
/// Consumed by: NexusMail.Worker.AI, NexusMail.Worker.Automation, NexusMail.Worker.Notification, NexusMail.Search
/// </summary>
public sealed record EmailReceivedMessage
{
    public Guid EmailId { get; init; }
    public Guid EmailAccountId { get; init; }
    public Guid WorkspaceId { get; init; }
    public string MessageId { get; init; } = string.Empty;  // Provider message ID
    public string Subject { get; init; } = string.Empty;
    public string Sender { get; init; } = string.Empty;
    public string SenderName { get; init; } = string.Empty;
    public string Preview { get; init; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; init; }
    public bool HasAttachments { get; init; }
    public IReadOnlyList<string> Labels { get; init; } = [];
    public string CorrelationId { get; init; } = string.Empty;
}
