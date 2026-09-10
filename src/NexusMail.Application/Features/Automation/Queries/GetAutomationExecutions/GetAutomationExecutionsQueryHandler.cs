using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Shared.Common;
using NexusMail.Application.Features.Automation.Abstractions;
using System.Collections.Generic;

namespace NexusMail.Application.Features.Automation.Queries.GetAutomationExecutions;

public sealed class GetAutomationExecutionsQueryHandler : IRequestHandler<GetAutomationExecutionsQuery, Result<List<AutomationExecutionDto>>>
{
    private readonly IAutomationExecutionRepository _repository;

    public GetAutomationExecutionsQueryHandler(IAutomationExecutionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<AutomationExecutionDto>>> Handle(GetAutomationExecutionsQuery request, CancellationToken cancellationToken)
    {
        var executions = await _repository.GetExecutionsByWorkspaceAsync(request.WorkspaceId, cancellationToken);
        
        var dtos = executions.Select(e => new AutomationExecutionDto
        {
            Id = e.Id,
            RuleId = e.RuleId,
            EmailId = e.EmailId,
            Status = e.Status,
            CreatedAt = e.CreatedAt
        }).ToList();

        return Result.Success(dtos);
    }
}
