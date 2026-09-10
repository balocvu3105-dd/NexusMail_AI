namespace NexusMail.Contracts.Search;

/// <summary>
/// Triggered when an email is successfully indexed into the search engine.
/// Consumed by: (Future) UI signalR, (Future) Analytics
/// </summary>
public sealed record SearchIndexedEvent
{
    public Guid EmailId { get; init; }
    public Guid WorkspaceId { get; init; }
    public DateTimeOffset IndexedAt { get; init; }
}
