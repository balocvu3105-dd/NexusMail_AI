using System;
using NexusMail.Shared.Events;

namespace NexusMail.Domain.Workspace.Events;

public sealed record WorkspaceCreated : EventBase
{
    public Guid WorkspaceId { get; init; }
    public string Plan { get; init; } = string.Empty;
    public Guid CreatorId { get; init; }
}

