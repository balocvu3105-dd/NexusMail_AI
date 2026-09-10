using System;
using NexusMail.Shared.Domain;

namespace NexusMail.Domain.Email.Events;

public sealed record EmailSyncFailedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOnUtc { get; } = DateTimeOffset.UtcNow;
    
    public Guid EmailAccountId { get; init; }
    public string ErrorMessage { get; init; }

    public EmailSyncFailedEvent(Guid emailAccountId, string errorMessage)
    {
        EmailAccountId = emailAccountId;
        ErrorMessage = errorMessage;
    }
}
