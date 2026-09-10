using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Shared.Common;
using NexusMail.Application.Features.Automation.Abstractions;
using NexusMail.Application.Features.Automation.Queries.GetAutomationRules;

namespace NexusMail.Application.Features.Automation.Queries.GetAutomationRuleById;

public sealed class GetAutomationRuleByIdQueryHandler : IRequestHandler<GetAutomationRuleByIdQuery, Result<AutomationRuleDto>>
{
    private readonly IAutomationRuleRepository _repository;

    public GetAutomationRuleByIdQueryHandler(IAutomationRuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AutomationRuleDto>> Handle(GetAutomationRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetRuleByIdAsync(request.RuleId, request.WorkspaceId, cancellationToken);
        
        if (rule == null)
        {
            return Result.Failure<AutomationRuleDto>(new Error("AutomationRule.NotFound", $"Automation rule with ID {request.RuleId} was not found in this workspace."));
        }

        var dto = new AutomationRuleDto
        {
            Id = rule.Id,
            Name = rule.Name,
            Description = rule.Description,
            TriggerType = rule.TriggerType.ToString(),
            ConditionsJson = rule.ConditionsJson,
            ActionsJson = rule.ActionsJson,
            IsEnabled = rule.IsEnabled,
            DryRun = rule.DryRun,
            ExecutionCount = rule.ExecutionCount,
            LastExecutedAt = rule.LastExecutedAt,
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt
        };

        return Result.Success(dto);
    }
}
