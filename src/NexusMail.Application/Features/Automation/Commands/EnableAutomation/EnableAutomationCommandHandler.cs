using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Shared.Common;
using NexusMail.Application.Features.Automation.Abstractions;

namespace NexusMail.Application.Features.Automation.Commands.EnableAutomation;

public sealed class EnableAutomationCommandHandler : IRequestHandler<EnableAutomationCommand, Result>
{
    private readonly IAutomationRuleRepository _repository;

    public EnableAutomationCommandHandler(IAutomationRuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(EnableAutomationCommand request, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetRuleByIdAsync(request.RuleId, request.WorkspaceId, cancellationToken);
        if (rule == null)
        {
            return Result.Failure(new Error("AutomationRule.NotFound", "Rule not found or does not belong to this workspace."));
        }

        rule.Enable();
        await _repository.UpdateRuleAsync(rule, cancellationToken);

        return Result.Success();
    }
}
