using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexusMail.Domain.Workspace.Repositories;
using NexusMail.Domain.Workspace.Entities;

namespace NexusMail.Infrastructure.Persistence.Repositories;

public sealed class WorkspaceRepository : IWorkspaceRepository
{
    private readonly ApplicationDbContext _dbContext;

    public WorkspaceRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Workspace workspace, CancellationToken cancellationToken = default)
    {
        await _dbContext.Workspaces.AddAsync(workspace, cancellationToken);
    }

    public async Task DeleteAsync(Workspace workspace, CancellationToken cancellationToken = default)
    {
        _dbContext.Workspaces.Remove(workspace);
        await Task.CompletedTask;
    }

    public async Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Workspaces.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<List<Workspace>> GetByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Workspaces
            .Include(w => w.Members)
            .Where(w => w.Members.Any(m => m.UserId == ownerId && m.Role == NexusMail.Domain.Workspace.Enums.WorkspaceRole.Owner))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid ownerId, string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Workspaces
            .AnyAsync(w => w.Name == name && w.Members.Any(m => m.UserId == ownerId && m.Role == NexusMail.Domain.Workspace.Enums.WorkspaceRole.Owner), cancellationToken);
    }

    public async Task<bool> ExistsMemberAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<WorkspaceMember>()
            .AnyAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId, cancellationToken);
    }

    public async Task<bool> IsOwnerAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<WorkspaceMember>()
            .AnyAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId && m.Role == NexusMail.Domain.Workspace.Enums.WorkspaceRole.Owner, cancellationToken);
    }

    public async Task<NexusMail.Domain.Workspace.Enums.WorkspaceRole?> GetRoleAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<WorkspaceMember>()
            .Where(m => m.WorkspaceId == workspaceId && m.UserId == userId)
            .Select(m => (NexusMail.Domain.Workspace.Enums.WorkspaceRole?)m.Role)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default)
    {
        _dbContext.Workspaces.Update(workspace);
        await Task.CompletedTask;
    }
}
