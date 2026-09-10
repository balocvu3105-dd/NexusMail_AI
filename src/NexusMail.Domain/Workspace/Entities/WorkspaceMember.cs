using System;
using NexusMail.Shared.Domain;
using NexusMail.Domain.Workspace.Enums;

namespace NexusMail.Domain.Workspace.Entities;

public sealed class WorkspaceMember : EntityBase<Guid>
{
    private WorkspaceMember(Guid id, Guid workspaceId, Guid userId, WorkspaceRole role) : base(id)
    {
        WorkspaceId = workspaceId;
        UserId = userId;
        Role = role;
        JoinedAtUtc = DateTimeOffset.UtcNow;
    }

    private WorkspaceMember() { } // For EF Core

    public Guid WorkspaceId { get; private set; }
    public Guid UserId { get; private set; }
    public WorkspaceRole Role { get; private set; }
    public DateTimeOffset JoinedAtUtc { get; private set; }

    public static WorkspaceMember Create(Guid workspaceId, Guid userId, WorkspaceRole role)
    {
        return new WorkspaceMember(Guid.NewGuid(), workspaceId, userId, role);
    }

    public void ChangeRole(WorkspaceRole newRole)
    {
        Role = newRole;
    }
}
