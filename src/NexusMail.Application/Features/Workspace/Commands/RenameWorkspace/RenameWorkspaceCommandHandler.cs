namespace NexusMail.Application.Features.Workspace.Commands.RenameWorkspace;

public sealed class RenameWorkspaceCommandHandler : IRequestHandler<RenameWorkspaceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RenameWorkspaceCommand request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(Result.Success(Guid.NewGuid()));
    }
}

