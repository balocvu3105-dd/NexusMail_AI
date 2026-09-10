using System;
using System.Collections.Generic;
using NexusMail.Shared.Events;

namespace NexusMail.Shared.Domain;

public abstract class AggregateRoot<TId> : EntityBase<TId>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() { }
    protected AggregateRoot(TId id) : base(id) { }

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

public abstract class AggregateRoot : AggregateRoot<Guid>
{
    protected AggregateRoot() : base(Guid.NewGuid()) { }
    protected AggregateRoot(Guid id) : base(id) { }
}
