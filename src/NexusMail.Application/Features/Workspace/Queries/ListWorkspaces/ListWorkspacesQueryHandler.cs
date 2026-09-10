namespace NexusMail.Application.Features.Workspace.Queries.ListWorkspaces;

public sealed class ListWorkspacesQueryHandler : IRequestHandler<ListWorkspacesQuery, Result<object>>
{
    public async Task<Result<object>> Handle(ListWorkspacesQuery request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(Result.Success(new object()));
    }
}

