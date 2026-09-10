using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Automation.Commands.CreateAutomationRule;

public sealed class CreateAutomationRuleCommandHandler : IRequestHandler<CreateAutomationRuleCommand, Result<Guid>>
{
    private readonly NexusMail.Application.Features.Automation.Abstractions.IAutomationRuleRepository _repository;

    public CreateAutomationRuleCommandHandler(NexusMail.Application.Features.Automation.Abstractions.IAutomationRuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateAutomationRuleCommand request, CancellationToken cancellationToken)
    {
        var conditionError = AutomationConfigurationValidator.ValidateConditions(request.ConditionsJson);
        if (conditionError != null) return Result.Failure<Guid>(conditionError);

        var actionError = AutomationConfigurationValidator.ValidateActions(request.ActionsJson);
        if (actionError != null) return Result.Failure<Guid>(actionError);

        var rule = NexusMail.Domain.Automation.Entities.AutomationRule.Create(
            request.WorkspaceId,
            request.Name,
            request.TriggerType,
            request.ConditionsJson,
            request.ActionsJson,
            request.Description,
            request.DryRun
        );

        await _repository.AddRuleAsync(rule, cancellationToken);

        return Result.Success(rule.Id);
    }
}

