using System;
using MediatR;

namespace NexusMail.Shared.Domain;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTimeOffset OccurredOnUtc { get; }
}
