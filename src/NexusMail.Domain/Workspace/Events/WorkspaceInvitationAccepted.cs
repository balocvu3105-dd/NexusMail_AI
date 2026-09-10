using System;
using NexusMail.Shared.Events;

namespace NexusMail.Domain.Workspace.Events;

public sealed record WorkspaceInvitationAccepted : EventBase
{
    public Guid WorkspaceId { get; init; }
    public Guid InvitationId { get; init; }
    public Guid UserId { get; init; }
}
