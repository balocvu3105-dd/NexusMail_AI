using System;
using System.Collections.Generic;
using System.Linq;
using NexusMail.Shared.Domain;
using NexusMail.Domain.Workspace.Events;
using NexusMail.Domain.Workspace.Enums;

namespace NexusMail.Domain.Workspace.Entities;

public sealed class Workspace : AggregateRoot<Guid>
{
    private readonly List<WorkspaceMember> _members = new();
    private readonly List<WorkspaceInvitation> _invitations = new();

    private Workspace(Guid id, string name, string plan) : base(id)
    {
        Name = name;
        Plan = plan;
    }

    private Workspace() { } // For EF Core

    public string Name { get; private set; } = string.Empty;
    public string Plan { get; private set; } = string.Empty;
    
    public IReadOnlyCollection<WorkspaceMember> Members => _members.AsReadOnly();
    public IReadOnlyCollection<WorkspaceInvitation> Invitations => _invitations.AsReadOnly();

    public static Workspace Create(string name, Guid creatorId, string plan = "Free")
    {
        Guard.Against.Empty(name);
        Guard.Against.Empty(plan);

        if (creatorId == Guid.Empty)
            throw new ArgumentException("CreatorId cannot be empty.", nameof(creatorId));

        var workspace = new Workspace(Guid.NewGuid(), name, plan);
        
        workspace.AddMember(creatorId, WorkspaceRole.Owner);

        workspace.AddDomainEvent(new WorkspaceCreated
        {
            WorkspaceId = workspace.Id,
            CreatorId = creatorId,
            Plan = workspace.Plan
        });

        return workspace;
    }

    public void AddMember(Guid userId, WorkspaceRole role)
    {
        if (_members.Any(m => m.UserId == userId))
        {
            throw new InvalidOperationException("User is already a member of this workspace.");
        }

        var member = WorkspaceMember.Create(Id, userId, role);
        _members.Add(member);
        
        AddDomainEvent(new WorkspaceMemberAdded { WorkspaceId = Id, UserId = userId, Role = role });
    }

    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member == null) return;

        if (member.Role == WorkspaceRole.Owner)
        {
            EnsureAtLeastOneOwnerRemains(userId);
        }

        _members.Remove(member);
        AddDomainEvent(new WorkspaceMemberRemoved { WorkspaceId = Id, UserId = userId });
    }

    public void ChangeMemberRole(Guid userId, WorkspaceRole newRole)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId) 
            ?? throw new InvalidOperationException("Member not found.");

        if (member.Role == WorkspaceRole.Owner && newRole != WorkspaceRole.Owner)
        {
            EnsureAtLeastOneOwnerRemains(userId);
        }

        member.ChangeRole(newRole);
        AddDomainEvent(new WorkspaceMemberRoleChanged { WorkspaceId = Id, UserId = userId, Role = newRole });
    }

    public WorkspaceInvitation InviteMember(string email, WorkspaceRole role, int expiryDays = 7)
    {
        Guard.Against.Empty(email);

        var activeInvitation = _invitations.FirstOrDefault(i => i.Email == email && i.Status == InvitationStatus.Pending);
        if (activeInvitation != null)
        {
            throw new InvalidOperationException("An active invitation already exists for this email.");
        }

        var token = Guid.NewGuid().ToString("N");
        var expiresAt = DateTimeOffset.UtcNow.AddDays(expiryDays);

        var invitation = WorkspaceInvitation.Create(Id, email, role, token, expiresAt);
        _invitations.Add(invitation);

        AddDomainEvent(new WorkspaceInvitationSent { WorkspaceId = Id, Email = email, Role = role, Token = token });
        
        return invitation;
    }

    public void AcceptInvitation(Guid invitationId, Guid userId)
    {
        var invitation = _invitations.FirstOrDefault(i => i.Id == invitationId) 
            ?? throw new InvalidOperationException("Invitation not found.");
            
        invitation.Accept();
        
        // When an invitation is accepted, they become a member
        AddMember(userId, invitation.Role);
        
        AddDomainEvent(new WorkspaceInvitationAccepted { WorkspaceId = Id, InvitationId = invitationId, UserId = userId });
    }

    public void RevokeInvitation(Guid invitationId)
    {
        var invitation = _invitations.FirstOrDefault(i => i.Id == invitationId) 
            ?? throw new InvalidOperationException("Invitation not found.");
            
        invitation.Revoke();
        AddDomainEvent(new WorkspaceInvitationRevoked { WorkspaceId = Id, InvitationId = invitationId });
    }

    private void EnsureAtLeastOneOwnerRemains(Guid excludingUserId)
    {
        var otherOwnersCount = _members.Count(m => m.Role == WorkspaceRole.Owner && m.UserId != excludingUserId);
        if (otherOwnersCount == 0)
        {
            throw new InvalidOperationException("Cannot remove or demote the last owner of the workspace.");
        }
    }
}
