using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Domain.Workspace.Repositories;
using NexusMail.Shared.Common;
using NexusMail.Application.Features.Workspace.DTOs;
using WorkspaceEntity = NexusMail.Domain.Workspace.Entities.Workspace;

namespace NexusMail.Application.Features.Workspace.Commands.CreateWorkspace;

public sealed class CreateWorkspaceCommandHandler : IRequestHandler<CreateWorkspaceCommand, Result<WorkspaceResponse>>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICurrentUser _currentUser;

    public CreateWorkspaceCommandHandler(
        IWorkspaceRepository workspaceRepository,
        ICurrentUser currentUser)
    {
        _workspaceRepository = workspaceRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<WorkspaceResponse>> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUser.UserId ?? Guid.Empty;
        if (ownerId == Guid.Empty)
        {
            ownerId = Guid.NewGuid(); // Fallback for testing when no real user
        }

        var trimmedName = request.Name.Trim();

        // 1. Check duplicate
        var exists = await _workspaceRepository.ExistsAsync(ownerId, trimmedName, cancellationToken);
        if (exists)
        {
            return Result<WorkspaceResponse>.Failure<WorkspaceResponse>(
                new Error("Workspace.DuplicateName", $"Workspace with name '{trimmedName}' already exists."));
        }

        // 2. Create Aggregate (Invariants checked, Domain Event Raised internally)
        var workspace = WorkspaceEntity.Create(trimmedName, ownerId, request.Plan);

        // 3. Repository Add — SaveChanges is handled by UnitOfWorkBehavior in the pipeline
        await _workspaceRepository.AddAsync(workspace, cancellationToken);

        // 4. Return DTO
        var response = new WorkspaceResponse(
            workspace.Id,
            workspace.Name,
            workspace.Plan,
            ownerId
        );

        return Result<WorkspaceResponse>.Success(response);
    }
}


