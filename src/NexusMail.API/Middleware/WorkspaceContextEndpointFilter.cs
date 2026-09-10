using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Domain.Workspace.Repositories;

namespace NexusMail.API.Middleware;

public sealed class WorkspaceContextEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var workspaceContext = context.HttpContext.RequestServices.GetRequiredService<IWorkspaceContext>();
        
        if (workspaceContext.WorkspaceId == null)
        {
            return Results.BadRequest(new { error = "Missing X-Workspace-Id header." });
        }

        var workspaceRepository = context.HttpContext.RequestServices.GetRequiredService<IWorkspaceRepository>();
        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUser>();

        var workspace = await workspaceRepository.GetByIdAsync(workspaceContext.WorkspaceId.Value, context.HttpContext.RequestAborted);
        if (workspace == null)
        {
            return Results.NotFound(new { error = "Workspace not found." });
        }

        if (currentUser.IsAuthenticated)
        {
            var role = await workspaceRepository.GetRoleAsync(workspaceContext.WorkspaceId.Value, currentUser.UserId!.Value, context.HttpContext.RequestAborted);
            if (role == null)
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }
        }

        return await next(context);
    }
}
