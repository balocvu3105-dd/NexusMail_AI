using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexusMail.Application.Features.Automation.Abstractions;
using NexusMail.Application.Features.Automation.DTOs;
using NexusMail.Common.Pagination;
using NexusMail.Domain.Automation.Entities;

namespace NexusMail.Infrastructure.Persistence.Repositories;

public sealed class AutomationAnalyticsRepository : IAutomationAnalyticsRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AutomationAnalyticsRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AutomationOverviewDto> GetOverviewAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var rulesQuery = _dbContext.AutomationRules.Where(r => r.WorkspaceId == workspaceId);
        
        var totalRules = await rulesQuery.CountAsync(cancellationToken);
        var activeRules = await rulesQuery.CountAsync(r => r.IsEnabled, cancellationToken);

        var executionsQuery = _dbContext.AutomationExecutions
            .Join(rulesQuery,
                  exec => exec.RuleId,
                  rule => rule.Id,
                  (exec, rule) => exec);

        var statusCounts = await executionsQuery
            .GroupBy(e => e.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        int succeeded = statusCounts.FirstOrDefault(x => x.Status == "Succeeded")?.Count ?? 0;
        int failed = statusCounts.FirstOrDefault(x => x.Status == "Failed")?.Count ?? 0;
        int unknown = statusCounts.FirstOrDefault(x => x.Status == "Unknown")?.Count ?? 0;
        int pending = statusCounts.FirstOrDefault(x => x.Status == "Pending")?.Count ?? 0;
        int executing = statusCounts.FirstOrDefault(x => x.Status == "Executing")?.Count ?? 0;

        int totalExecutions = succeeded + failed + unknown + pending + executing;
        int terminalExecutions = succeeded + failed + unknown;
        double successRate = terminalExecutions > 0 ? (double)succeeded / terminalExecutions : 0;

        return new AutomationOverviewDto(
            TotalRules: totalRules,
            ActiveRules: activeRules,
            TotalExecutions: totalExecutions,
            SucceededExecutions: succeeded,
            FailedExecutions: failed,
            UnknownExecutions: unknown,
            PendingExecutions: pending + executing,
            SuccessRate: successRate
        );
    }

    public async Task<PagedList<ExecutionAnalyticsDto>> GetExecutionsAsync(
        Guid workspaceId, 
        DateTimeOffset? fromDate, 
        DateTimeOffset? toDate, 
        Guid? ruleId, 
        string? status, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var rulesQuery = _dbContext.AutomationRules.Where(r => r.WorkspaceId == workspaceId);
        if (ruleId.HasValue)
        {
            rulesQuery = rulesQuery.Where(r => r.Id == ruleId.Value);
        }

        var query = _dbContext.AutomationExecutions
            .Join(rulesQuery,
                  exec => exec.RuleId,
                  rule => rule.Id,
                  (exec, rule) => new { exec, rule });

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.exec.CreatedAt >= fromDate.Value);
        }
        
        if (toDate.HasValue)
        {
            query = query.Where(x => x.exec.CreatedAt <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.exec.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(x => x.exec.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ExecutionAnalyticsDto(
                x.exec.Id,
                x.rule.Id,
                x.rule.Name,
                x.exec.EmailId,
                x.exec.Status,
                x.exec.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedList<ExecutionAnalyticsDto>(items, totalCount, page, pageSize);
    }

    public async Task<RuleAnalyticsDto?> GetRuleAnalyticsAsync(Guid workspaceId, Guid ruleId, CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.AutomationRules
            .FirstOrDefaultAsync(r => r.WorkspaceId == workspaceId && r.Id == ruleId, cancellationToken);
            
        if (rule == null)
            return null;

        var statusCounts = await _dbContext.AutomationExecutions
            .Where(e => e.RuleId == ruleId)
            .GroupBy(e => e.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        int succeeded = statusCounts.FirstOrDefault(x => x.Status == "Succeeded")?.Count ?? 0;
        int failed = statusCounts.FirstOrDefault(x => x.Status == "Failed")?.Count ?? 0;
        int unknown = statusCounts.FirstOrDefault(x => x.Status == "Unknown")?.Count ?? 0;
        int terminalExecutions = succeeded + failed + unknown;
        int totalExecutions = statusCounts.Sum(x => x.Count);
        double successRate = terminalExecutions > 0 ? (double)succeeded / terminalExecutions : 0;

        return new RuleAnalyticsDto(
            RuleId: rule.Id,
            RuleName: rule.Name,
            TotalExecutions: totalExecutions,
            SucceededExecutions: succeeded,
            FailedExecutions: failed,
            UnknownExecutions: unknown,
            SuccessRate: successRate,
            LastExecutedAt: rule.LastExecutedAt
        );
    }
}
