namespace NexusMail.Contracts.Email;

/// <summary>
/// Message published to the bus when an email sync is requested for an account.
/// Consumed by: NexusMail.Worker.EmailSync
/// </summary>
public sealed record EmailSyncRequestedMessage
{
    public Guid EmailAccountId { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
    public DateTimeOffset RequestedAt { get; init; } = DateTimeOffset.UtcNow;
    public bool IsFullSync { get; init; } = false; // false = incremental (delta)
}
