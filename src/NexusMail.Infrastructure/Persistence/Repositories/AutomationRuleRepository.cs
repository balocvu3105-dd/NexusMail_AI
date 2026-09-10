using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexusMail.Application.Features.Automation.Abstractions;
using NexusMail.Domain.Automation.Entities;

namespace NexusMail.Infrastructure.Persistence.Repositories;

public sealed class AutomationRuleRepository : IAutomationRuleRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AutomationRuleRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AutomationRule>> GetRulesByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AutomationRules
            .Where(r => r.WorkspaceId == workspaceId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<AutomationRule?> GetRuleByIdAsync(Guid ruleId, Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AutomationRules
            .FirstOrDefaultAsync(r => r.Id == ruleId && r.WorkspaceId == workspaceId, cancellationToken);
    }

    public async Task AddRuleAsync(AutomationRule rule, CancellationToken cancellationToken = default)
    {
        _dbContext.AutomationRules.Add(rule);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRuleAsync(AutomationRule rule, CancellationToken cancellationToken = default)
    {
        _dbContext.AutomationRules.Update(rule);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
