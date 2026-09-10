using System;

using NexusMail.Shared.Domain;

namespace NexusMail.Shared.Events;

public abstract record EventBase : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOnUtc { get; init; } = DateTimeOffset.UtcNow;
}
