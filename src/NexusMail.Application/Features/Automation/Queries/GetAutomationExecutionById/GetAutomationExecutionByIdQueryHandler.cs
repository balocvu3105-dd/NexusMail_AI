using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Shared.Common;
using NexusMail.Application.Features.Automation.Abstractions;

namespace NexusMail.Application.Features.Automation.Queries.GetAutomationExecutionById;

public sealed class GetAutomationExecutionByIdQueryHandler : IRequestHandler<GetAutomationExecutionByIdQuery, Result<AutomationExecutionDetailDto>>
{
    private readonly IAutomationExecutionRepository _repository;

    public GetAutomationExecutionByIdQueryHandler(IAutomationExecutionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AutomationExecutionDetailDto>> Handle(GetAutomationExecutionByIdQuery request, CancellationToken cancellationToken)
    {
        var execution = await _repository.GetExecutionByIdAsync(request.ExecutionId, request.WorkspaceId, cancellationToken);
        
        if (execution == null)
        {
            return Result.Failure<AutomationExecutionDetailDto>(new Error("AutomationExecution.NotFound", $"Execution with ID {request.ExecutionId} was not found in this workspace."));
        }

        var actionExecutions = await _repository.GetActionExecutionsAsync(request.ExecutionId, cancellationToken);
        var audits = await _repository.GetAuditsByExecutionAsync(request.ExecutionId, cancellationToken);

        var dto = new AutomationExecutionDetailDto
        {
            Id = execution.Id,
            RuleId = execution.RuleId,
            EmailId = execution.EmailId,
            Status = execution.Status,
            CreatedAt = execution.CreatedAt,
            ActionExecutions = actionExecutions.Select(a => new ActionExecutionDto
            {
                ActionKey = a.ActionKey,
                ActionType = a.ActionType,
                Status = a.Status,
                CreatedAt = a.CreatedAt,
                CompletedAt = a.CompletedAt,
                RetryCount = a.RetryCount,
                ErrorMessage = a.ErrorMessage
            }).ToList(),
            Audits = audits.Select(a => new AuditDto
            {
                Executor = a.Executor,
                Result = a.Result,
                ErrorMessage = a.ErrorMessage,
                ExecutedAt = a.ExecutedAt
            }).ToList()
        };

        return Result.Success(dto);
    }
}
