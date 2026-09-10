using System;
using NexusMail.Shared.Domain;

namespace NexusMail.Domain.Email.Events;

public record EmailAccountConnectedEvent(Guid EmailAccountId, Guid WorkspaceId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOnUtc { get; init; } = DateTimeOffset.UtcNow;
}


