using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Domain.Workspace.Repositories;

namespace NexusMail.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICurrentUser _currentUser;

    public NotificationHub(IWorkspaceRepository workspaceRepository, ICurrentUser currentUser)
    {
        _workspaceRepository = workspaceRepository;
        _currentUser = currentUser;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext == null)
        {
            Context.Abort();
            return;
        }

        var workspaceIdString = httpContext.Request.Query["workspaceId"].ToString();
        if (!Guid.TryParse(workspaceIdString, out var workspaceId))
        {
            Context.Abort();
            return;
        }

        var userId = _currentUser.UserId;
        if (userId == null || userId == Guid.Empty)
        {
            throw new HubException("Unauthorized");
        }

        var role = await _workspaceRepository.GetRoleAsync(workspaceId, userId.Value);
        if (role == null)
        {
            // User does not have access to this workspace
            throw new HubException("Forbidden");
        }

        // Add user to the workspace group
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Workspace-{workspaceId}");

        await base.OnConnectedAsync();
    }
}
