using System.Threading;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NexusMail.API.Infrastructure.Extensions;
using NexusMail.Application.Abstractions.Time;
using NexusMail.Application.Features.Workspace.Commands.CreateWorkspace;

namespace NexusMail.API.Endpoints.Workspace;

public static class CreateWorkspaceEndpoint
{
    public static void MapCreateWorkspaceEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/v{version:apiVersion}/workspaces", async (
            CreateWorkspaceCommand command, 
            ISender sender, 
            HttpContext context,
            IClock clock,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.ToCreatedResult($"/api/v1/workspaces/{result.Value?.Id}", context, clock);
        })
        .WithTags("Workspaces")
        .MapToApiVersion(1, 0);
    }
}
