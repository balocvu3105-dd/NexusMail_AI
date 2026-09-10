namespace NexusMail.Events.Integration;

/// <summary>
/// Marker interface for Integration Events.
///
/// Integration Events cross service or bounded-context boundaries.
/// They are published to the message bus (e.g. RabbitMQ via MassTransit).
///
/// KEY DISTINCTION from Domain Events:
///   - Domain Events   → in-process, handled within same transaction, MediatR INotification
///   - Integration Events → cross-process, at-least-once delivery, message bus
///
/// RULE: Integration Events MUST be idempotent — consumers handle duplicates gracefully.
/// RULE: Integration Events MUST NOT carry domain objects — use primitives or Contracts DTOs.
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredOnUtc { get; }
    string CorrelationId { get; }
}
