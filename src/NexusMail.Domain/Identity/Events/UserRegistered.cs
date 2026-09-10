using System;
using NexusMail.Shared.Events;

namespace NexusMail.Domain.Identity.Events;

public sealed record UserRegistered : EventBase
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
}

