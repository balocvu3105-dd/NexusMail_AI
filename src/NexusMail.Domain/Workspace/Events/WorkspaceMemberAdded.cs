using System;
using NexusMail.Shared.Events;
using NexusMail.Domain.Workspace.Enums;

namespace NexusMail.Domain.Workspace.Events;

public sealed record WorkspaceMemberAdded : EventBase
{
    public Guid WorkspaceId { get; init; }
    public Guid UserId { get; init; }
    public WorkspaceRole Role { get; init; }
}
