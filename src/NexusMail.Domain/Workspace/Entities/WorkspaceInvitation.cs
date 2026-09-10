using System;
using NexusMail.Shared.Domain;
using NexusMail.Domain.Workspace.Enums;

namespace NexusMail.Domain.Workspace.Entities;

public sealed class WorkspaceInvitation : EntityBase<Guid>
{
    private WorkspaceInvitation(Guid id, Guid workspaceId, string email, WorkspaceRole role, string token, DateTimeOffset expiresAtUtc) : base(id)
    {
        WorkspaceId = workspaceId;
        Email = email;
        Role = role;
        Token = token;
        Status = InvitationStatus.Pending;
        InvitedAtUtc = DateTimeOffset.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
    }

    private WorkspaceInvitation() { } // For EF Core

    public Guid WorkspaceId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public WorkspaceRole Role { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public InvitationStatus Status { get; private set; }
    public DateTimeOffset InvitedAtUtc { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public static WorkspaceInvitation Create(Guid workspaceId, string email, WorkspaceRole role, string token, DateTimeOffset expiresAtUtc)
    {
        Guard.Against.Empty(email);
        Guard.Against.Empty(token);

        return new WorkspaceInvitation(Guid.NewGuid(), workspaceId, email, role, token, expiresAtUtc);
    }

    public void Accept()
    {
        if (Status != InvitationStatus.Pending)
            throw new InvalidOperationException("Only pending invitations can be accepted.");
        
        if (DateTimeOffset.UtcNow > ExpiresAtUtc)
        {
            Status = InvitationStatus.Expired;
            throw new InvalidOperationException("Invitation has expired.");
        }

        Status = InvitationStatus.Accepted;
    }

    public void Revoke()
    {
        if (Status != InvitationStatus.Pending)
            throw new InvalidOperationException("Only pending invitations can be revoked.");

        Status = InvitationStatus.Revoked;
    }
}
