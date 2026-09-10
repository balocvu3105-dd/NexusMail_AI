using System;
using NexusMail.Shared.Domain;

namespace NexusMail.Domain.Email.Events;

public sealed record EmailAccountDisconnectedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOnUtc { get; } = DateTimeOffset.UtcNow;
    
    public Guid EmailAccountId { get; init; }

    public EmailAccountDisconnectedEvent(Guid emailAccountId)
    {
        EmailAccountId = emailAccountId;
    }
}
