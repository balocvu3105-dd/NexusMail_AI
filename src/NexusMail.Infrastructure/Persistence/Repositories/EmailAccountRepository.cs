using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Domain.Email.Entities;

namespace NexusMail.Infrastructure.Persistence.Repositories;

public sealed class EmailAccountRepository : IEmailAccountRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EmailAccountRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EmailAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmailAccounts
            .Include(e => e.State)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<(List<EmailAccount> Items, int TotalCount)> GetPagedByWorkspaceIdAsync(Guid workspaceId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.EmailAccounts
            .AsNoTracking()
            .Where(e => e.WorkspaceId == workspaceId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(EmailAccount emailAccount, CancellationToken cancellationToken = default)
    {
        await _dbContext.EmailAccounts.AddAsync(emailAccount, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(EmailAccount emailAccount, CancellationToken cancellationToken = default)
    {
        _dbContext.EmailAccounts.Update(emailAccount);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(EmailAccount emailAccount, CancellationToken cancellationToken = default)
    {
        _dbContext.EmailAccounts.Remove(emailAccount);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid workspaceId, string emailAddress, CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmailAccounts
            .AnyAsync(e => e.WorkspaceId == workspaceId && e.EmailAddress == emailAddress, cancellationToken);
    }

    public async Task<List<EmailAccount>> GetAccountsDueForSyncAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var query = from e in _dbContext.EmailAccounts
                    join s in _dbContext.EmailSynchronizationStates on e.Id equals s.EmailAccountId
                    where s.Status != NexusMail.Domain.Email.Enums.EmailSyncStatus.Syncing
                    orderby s.LastAttemptAt
                    select e;

        return await query.Take(batchSize).ToListAsync(cancellationToken);
    }
}
