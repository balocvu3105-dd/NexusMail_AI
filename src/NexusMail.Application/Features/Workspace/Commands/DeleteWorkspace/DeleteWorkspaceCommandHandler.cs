namespace NexusMail.Application.Features.Workspace.Commands.DeleteWorkspace;

public sealed class DeleteWorkspaceCommandHandler : IRequestHandler<DeleteWorkspaceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(DeleteWorkspaceCommand request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(Result.Success(Guid.NewGuid()));
    }
}

