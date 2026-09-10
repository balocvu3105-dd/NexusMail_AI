namespace NexusMail.Application.Features.Automation.Commands.RunAutomation;

public sealed class RunAutomationCommandHandler : IRequestHandler<RunAutomationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RunAutomationCommand request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(Result.Success(Guid.NewGuid()));
    }
}

