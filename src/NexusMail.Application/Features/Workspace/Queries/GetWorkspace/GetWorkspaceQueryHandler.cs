namespace NexusMail.Application.Features.Workspace.Queries.GetWorkspace;

public sealed class GetWorkspaceQueryHandler : IRequestHandler<GetWorkspaceQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetWorkspaceQuery request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(Result.Success(new object()));
    }
}

