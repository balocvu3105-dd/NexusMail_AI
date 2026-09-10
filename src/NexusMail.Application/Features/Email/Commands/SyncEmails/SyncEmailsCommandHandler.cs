namespace NexusMail.Application.Features.Email.Commands.SyncEmails;

public sealed class SyncEmailsCommandHandler : IRequestHandler<SyncEmailsCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SyncEmailsCommand request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(Result.Success(Guid.NewGuid()));
    }
}

