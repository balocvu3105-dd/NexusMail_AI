using System;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Application.Features.Automation.DTOs;
using NexusMail.Common.Pagination;

namespace NexusMail.Application.Features.Automation.Abstractions;

public interface IAutomationAnalyticsRepository
{
    Task<AutomationOverviewDto> GetOverviewAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    
    Task<PagedList<ExecutionAnalyticsDto>> GetExecutionsAsync(
        Guid workspaceId, 
        DateTimeOffset? fromDate, 
        DateTimeOffset? toDate, 
        Guid? ruleId, 
        string? status, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default);
        
    Task<RuleAnalyticsDto?> GetRuleAnalyticsAsync(Guid workspaceId, Guid ruleId, CancellationToken cancellationToken = default);
}
