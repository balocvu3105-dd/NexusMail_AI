using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Shared.Common;
using NexusMail.Application.Features.Automation.Abstractions;

namespace NexusMail.Application.Features.Automation.Commands.DisableAutomation;

public sealed class DisableAutomationCommandHandler : IRequestHandler<DisableAutomationCommand, Result>
{
    private readonly IAutomationRuleRepository _repository;

    public DisableAutomationCommandHandler(IAutomationRuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DisableAutomationCommand request, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetRuleByIdAsync(request.RuleId, request.WorkspaceId, cancellationToken);
        if (rule == null)
        {
            return Result.Failure(new Error("AutomationRule.NotFound", "Rule not found or does not belong to this workspace."));
        }

        rule.Disable();
        await _repository.UpdateRuleAsync(rule, cancellationToken);

        return Result.Success();
    }
}
