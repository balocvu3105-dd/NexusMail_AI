using System;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Application.Abstractions.Authorization;
using NexusMail.Domain.Workspace.Repositories;
using NexusMail.Domain.Workspace.Enums;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Authorization;

public sealed class WorkspaceAuthorizationService : IWorkspaceAuthorizationService
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public WorkspaceAuthorizationService(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    public async Task<Result> CanInviteAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
    {
        var role = await _workspaceRepository.GetRoleAsync(workspaceId, userId, cancellationToken);
        
        if (role == WorkspaceRole.Owner || role == WorkspaceRole.Admin)
            return Result.Success();
            
        return Result.Failure(new Error("Authorization.Forbidden", "You do not have permission to invite members."));
    }

    public async Task<Result> CanRemoveMemberAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
    {
        var role = await _workspaceRepository.GetRoleAsync(workspaceId, userId, cancellationToken);
        
        if (role == WorkspaceRole.Owner || role == WorkspaceRole.Admin)
            return Result.Success();
            
        return Result.Failure(new Error("Authorization.Forbidden", "You do not have permission to remove members."));
    }

    public async Task<Result> CanManageBillingAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
    {
        var isOwner = await _workspaceRepository.IsOwnerAsync(workspaceId, userId, cancellationToken);
        
        if (isOwner)
            return Result.Success();
            
        return Result.Failure(new Error("Authorization.Forbidden", "Only workspace owners can manage billing."));
    }

    public async Task<Result> CanCreateAutomationAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
    {
        var role = await _workspaceRepository.GetRoleAsync(workspaceId, userId, cancellationToken);
        
        // Let's assume Owner, Admin, Editor can create automations
        if (role == WorkspaceRole.Owner || role == WorkspaceRole.Admin || role == WorkspaceRole.Editor)
            return Result.Success();
            
        return Result.Failure(new Error("Authorization.Forbidden", "You do not have permission to create automations."));
    }

    public async Task<Result> CanDeleteWorkspaceAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
    {
        var isOwner = await _workspaceRepository.IsOwnerAsync(workspaceId, userId, cancellationToken);
        
        if (isOwner)
            return Result.Success();
            
        return Result.Failure(new Error("Authorization.Forbidden", "Only workspace owners can delete the workspace."));
    }
}
