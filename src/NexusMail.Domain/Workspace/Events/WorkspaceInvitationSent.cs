using System;
using NexusMail.Shared.Events;
using NexusMail.Domain.Workspace.Enums;

namespace NexusMail.Domain.Workspace.Events;

public sealed record WorkspaceInvitationSent : EventBase
{
    public Guid WorkspaceId { get; init; }
    public string Email { get; init; } = string.Empty;
    public WorkspaceRole Role { get; init; }
    public string Token { get; init; } = string.Empty;
}
