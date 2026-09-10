using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexusMail.Application.Features.Automation.Abstractions;
using NexusMail.Domain.Automation.Entities;

namespace NexusMail.Infrastructure.Persistence.Repositories;

public sealed class AutomationExecutionRepository : IAutomationExecutionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AutomationExecutionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AutomationExecution>> GetExecutionsByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        // AutomationExecution doesn't have WorkspaceId directly in Step 26.
        // It has RuleId. We must join with AutomationRules to filter by workspace.
        return await _dbContext.AutomationExecutions
            .Join(_dbContext.AutomationRules.Where(r => r.WorkspaceId == workspaceId),
                  exec => exec.RuleId,
                  rule => rule.Id,
                  (exec, rule) => exec)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<AutomationExecution?> GetExecutionByIdAsync(Guid executionId, Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AutomationExecutions
            .Join(_dbContext.AutomationRules.Where(r => r.WorkspaceId == workspaceId),
                  exec => exec.RuleId,
                  rule => rule.Id,
                  (exec, rule) => exec)
            .FirstOrDefaultAsync(e => e.Id == executionId, cancellationToken);
    }

    public async Task<List<AutomationActionExecution>> GetActionExecutionsAsync(Guid executionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AutomationActionExecutions
            .Where(a => a.ExecutionId == executionId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AutomationAudit>> GetAuditsByExecutionAsync(Guid executionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AutomationAudits
            .Where(a => a.ExecutionId == executionId)
            .OrderBy(a => a.ExecutedAt)
            .ToListAsync(cancellationToken);
    }
}
