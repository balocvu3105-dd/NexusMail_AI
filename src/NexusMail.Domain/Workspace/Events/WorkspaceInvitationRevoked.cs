using System;
using NexusMail.Shared.Events;

namespace NexusMail.Domain.Workspace.Events;

public sealed record WorkspaceInvitationRevoked : EventBase
{
    public Guid WorkspaceId { get; init; }
    public Guid InvitationId { get; init; }
}
