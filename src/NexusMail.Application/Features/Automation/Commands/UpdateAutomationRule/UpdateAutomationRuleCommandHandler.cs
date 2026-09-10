using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Automation.Commands.UpdateAutomationRule;

public sealed class UpdateAutomationRuleCommandHandler : IRequestHandler<UpdateAutomationRuleCommand, Result>
{
    private readonly NexusMail.Application.Features.Automation.Abstractions.IAutomationRuleRepository _repository;

    public UpdateAutomationRuleCommandHandler(NexusMail.Application.Features.Automation.Abstractions.IAutomationRuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateAutomationRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetRuleByIdAsync(request.RuleId, request.WorkspaceId, cancellationToken);
        if (rule == null)
        {
            return Result.Failure(new Error("AutomationRule.NotFound", "Rule not found or does not belong to this workspace."));
        }

        var conditionError = AutomationConfigurationValidator.ValidateConditions(request.ConditionsJson);
        if (conditionError != null) return Result.Failure(conditionError);

        var actionError = AutomationConfigurationValidator.ValidateActions(request.ActionsJson);
        if (actionError != null) return Result.Failure(actionError);

        rule.Update(request.Name, request.Description, request.ConditionsJson, request.ActionsJson);

        await _repository.UpdateRuleAsync(rule, cancellationToken);

        return Result.Success();
    }
}
