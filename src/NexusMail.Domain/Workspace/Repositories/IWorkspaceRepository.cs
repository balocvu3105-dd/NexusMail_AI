using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using WorkspaceEntity = NexusMail.Domain.Workspace.Entities.Workspace;

namespace NexusMail.Domain.Workspace.Repositories;

public interface IWorkspaceRepository
{
    Task<WorkspaceEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<WorkspaceEntity>> GetByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid ownerId, string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsMemberAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsOwnerAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
    Task<NexusMail.Domain.Workspace.Enums.WorkspaceRole?> GetRoleAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(WorkspaceEntity workspace, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkspaceEntity workspace, CancellationToken cancellationToken = default);
    Task DeleteAsync(WorkspaceEntity workspace, CancellationToken cancellationToken = default);
}
