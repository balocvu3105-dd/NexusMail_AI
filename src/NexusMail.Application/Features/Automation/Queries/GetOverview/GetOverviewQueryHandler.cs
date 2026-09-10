using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Features.Automation.Abstractions;
using NexusMail.Application.Features.Automation.DTOs;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.Automation.Queries.GetOverview;

public sealed class GetOverviewQueryHandler : IRequestHandler<GetOverviewQuery, Result<AutomationOverviewDto>>
{
    private readonly IAutomationAnalyticsRepository _analyticsRepository;

    public GetOverviewQueryHandler(IAutomationAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    public async Task<Result<AutomationOverviewDto>> Handle(GetOverviewQuery request, CancellationToken cancellationToken)
    {
        var overview = await _analyticsRepository.GetOverviewAsync(request.WorkspaceId, cancellationToken);
        return Result.Success(overview);
    }
}
