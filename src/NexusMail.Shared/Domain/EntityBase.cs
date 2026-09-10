using System;
using System.Collections.Generic;

namespace NexusMail.Shared.Domain;

public abstract class EntityBase<TId> : IEquatable<EntityBase<TId>>
{
    public TId Id { get; protected set; } = default!;

    protected EntityBase() { }

    protected EntityBase(TId id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((EntityBase<TId>)obj);
    }

    public bool Equals(EntityBase<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Id is null || other.Id is null) return false;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        return Id?.GetHashCode() ?? 0;
    }

    public static bool operator ==(EntityBase<TId>? left, EntityBase<TId>? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left?.Equals(right) ?? false;
    }

    public static bool operator !=(EntityBase<TId>? left, EntityBase<TId>? right)
        => !(left == right);
}

public abstract class EntityBase : EntityBase<Guid>
{
    protected EntityBase() : base(Guid.NewGuid()) { }
    protected EntityBase(Guid id) : base(id) { }
}
