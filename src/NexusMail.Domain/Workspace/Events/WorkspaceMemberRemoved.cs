using System;
using NexusMail.Shared.Events;

namespace NexusMail.Domain.Workspace.Events;

public sealed record WorkspaceMemberRemoved : EventBase
{
    public Guid WorkspaceId { get; init; }
    public Guid UserId { get; init; }
}
