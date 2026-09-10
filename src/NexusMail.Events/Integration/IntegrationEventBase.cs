namespace NexusMail.Events.Integration;

/// <summary>
/// Base class for all Integration Events.
/// Use this as the base for events published to the message bus.
/// </summary>
public abstract record IntegrationEventBase : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOnUtc { get; init; } = DateTimeOffset.UtcNow;
    public string CorrelationId { get; init; } = string.Empty;
}
