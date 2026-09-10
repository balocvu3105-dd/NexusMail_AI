using System;
using NexusMail.Shared.Domain;

namespace NexusMail.Domain.Email.Events;

public sealed record EmailSyncCompletedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOnUtc { get; } = DateTimeOffset.UtcNow;
    
    public Guid EmailAccountId { get; init; }
    public int EmailsSyncedCount { get; init; }

    public EmailSyncCompletedEvent(Guid emailAccountId, int emailsSyncedCount)
    {
        EmailAccountId = emailAccountId;
        EmailsSyncedCount = emailsSyncedCount;
    }
}
