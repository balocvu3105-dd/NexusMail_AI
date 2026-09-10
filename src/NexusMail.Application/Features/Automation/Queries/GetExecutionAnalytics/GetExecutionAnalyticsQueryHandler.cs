using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Features.Automation.Abstractions;
using NexusMail.Application.Features.Automation.DTOs;
using NexusMail.Common.Pagination;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.Automation.Queries.GetExecutionAnalytics;

public sealed class GetExecutionAnalyticsQueryHandler : IRequestHandler<GetExecutionAnalyticsQuery, Result<PagedList<ExecutionAnalyticsDto>>>
{
    private readonly IAutomationAnalyticsRepository _analyticsRepository;

    public GetExecutionAnalyticsQueryHandler(IAutomationAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    public async Task<Result<PagedList<ExecutionAnalyticsDto>>> Handle(GetExecutionAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var executions = await _analyticsRepository.GetExecutionsAsync(
            request.WorkspaceId,
            request.FromDate,
            request.ToDate,
            request.RuleId,
            request.Status,
            request.Page,
            request.PageSize,
            cancellationToken
        );

        return Result.Success(executions);
    }
}
