using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NexusMail.API.Middleware;

namespace NexusMail.API.Endpoints.Workspace;

public static class GetWorkspaceEndpoints
{
    public static void MapGetWorkspaceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/v{version:apiVersion}/workspaces/test-auth", () => Results.Ok())
            .RequireAuthorization()
            .AddEndpointFilter<WorkspaceContextEndpointFilter>()
            .WithTags("Workspaces")
            .MapToApiVersion(1, 0);
    }
}
