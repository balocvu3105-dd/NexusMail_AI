using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Features.Automation.Abstractions;
using NexusMail.Application.Features.Automation.DTOs;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.Automation.Queries.GetRuleAnalytics;

public sealed class GetRuleAnalyticsQueryHandler : IRequestHandler<GetRuleAnalyticsQuery, Result<RuleAnalyticsDto>>
{
    private readonly IAutomationAnalyticsRepository _analyticsRepository;

    public GetRuleAnalyticsQueryHandler(IAutomationAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    public async Task<Result<RuleAnalyticsDto>> Handle(GetRuleAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var analytics = await _analyticsRepository.GetRuleAnalyticsAsync(request.WorkspaceId, request.RuleId, cancellationToken);
        
        if (analytics == null)
            return Result.Failure<RuleAnalyticsDto>(new Error("Rule.NotFound", "The rule was not found."));

        return Result.Success(analytics);
    }
}
