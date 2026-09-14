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
        if (rule.RuleVersion != request.ExpectedRuleVersion)
        {
            return Result.Failure(new Error("AutomationRule.Concurrency", "The rule was modified by another user. Please reload the latest version."));
        }

        rule.Disable();
        
        try
        {
            await _repository.UpdateRuleAsync(rule, cancellationToken);
        }
        catch (NexusMail.Domain.Exceptions.ConcurrencyException)
        {
            return Result.Failure(new Error("AutomationRule.Concurrency", "The rule was modified by another user. Please reload the latest version."));
        }

        return Result.Success();
    }
}
