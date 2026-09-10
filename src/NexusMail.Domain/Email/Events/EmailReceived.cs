using System;
using NexusMail.Shared.Events;

namespace NexusMail.Domain.Email.Events;

public sealed record EmailReceived : EventBase
{
    public Guid EmailId { get; init; }
    public Guid AccountId { get; init; }

    /// <summary>
    /// Workspace that owns this email. Must be propagated to all downstream messages
    /// (AI, Automation, Search) to maintain workspace isolation boundary.
    /// </summary>
    public Guid WorkspaceId { get; init; }

    public string Sender { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; init; }
}

