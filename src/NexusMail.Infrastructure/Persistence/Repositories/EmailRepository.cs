using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Domain.AI.Entities;

namespace NexusMail.Infrastructure.Persistence.Repositories;

internal sealed class EmailRepository : IEmailRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EmailRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NexusMail.Domain.Email.Entities.Email?> GetByIdAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await (from email in _dbContext.Emails
                      join account in _dbContext.EmailAccounts on email.AccountId equals account.Id
                      where email.Id == id && account.WorkspaceId == workspaceId
                      select email)
                      .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(NexusMail.Domain.Email.Entities.Email Email, AIAnalysis? Analysis)?> GetEmailWithAnalysisAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var result = await (from email in _dbContext.Emails
                            join account in _dbContext.EmailAccounts on email.AccountId equals account.Id
                            where email.Id == id && account.WorkspaceId == workspaceId
                            join ai in _dbContext.AIAnalyses on email.Id equals ai.EmailId into aiGroup
                            from ai in aiGroup.DefaultIfEmpty()
                            select new { Email = email, Analysis = ai })
                            .FirstOrDefaultAsync(cancellationToken);

        if (result == null) return null;

        return (result.Email, result.Analysis);
    }

    public async Task<List<NexusMail.Domain.Email.Entities.Email>> GetByIdsAsync(IEnumerable<Guid> ids, Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await (from email in _dbContext.Emails
                      join account in _dbContext.EmailAccounts on email.AccountId equals account.Id
                      where ids.Contains(email.Id) && account.WorkspaceId == workspaceId
                      select email)
                      .ToListAsync(cancellationToken);
    }

    public async Task AddAnalysisAsync(AIAnalysis analysis, CancellationToken cancellationToken = default)
    {
        await _dbContext.AIAnalyses.AddAsync(analysis, cancellationToken);
    }

    public void UpdateAnalysis(AIAnalysis analysis)
    {
        _dbContext.AIAnalyses.Update(analysis);
    }
}
