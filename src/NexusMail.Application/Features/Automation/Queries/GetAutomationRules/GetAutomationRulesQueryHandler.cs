using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Shared.Common;
using NexusMail.Application.Features.Automation.Abstractions;
using System.Collections.Generic;

namespace NexusMail.Application.Features.Automation.Queries.GetAutomationRules;

public sealed class GetAutomationRulesQueryHandler : IRequestHandler<GetAutomationRulesQuery, Result<List<AutomationRuleDto>>>
{
    private readonly IAutomationRuleRepository _repository;

    public GetAutomationRulesQueryHandler(IAutomationRuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<AutomationRuleDto>>> Handle(GetAutomationRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _repository.GetRulesByWorkspaceAsync(request.WorkspaceId, cancellationToken);
        
        var dtos = rules.Select(r => new AutomationRuleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            TriggerType = r.TriggerType.ToString(),
            ConditionsJson = r.ConditionsJson,
            ActionsJson = r.ActionsJson,
            IsEnabled = r.IsEnabled,
            DryRun = r.DryRun,
            ExecutionCount = r.ExecutionCount,
            LastExecutedAt = r.LastExecutedAt,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList();

        return Result.Success(dtos);
    }
}
