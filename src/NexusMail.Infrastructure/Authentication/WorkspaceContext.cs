using System;
using Microsoft.AspNetCore.Http;
using NexusMail.Application.Abstractions.Authentication;

namespace NexusMail.Infrastructure.Authentication;

public sealed class WorkspaceContext : IWorkspaceContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WorkspaceContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? WorkspaceId
    {
        get
        {
            var headerValue = _httpContextAccessor.HttpContext?.Request.Headers["X-Workspace-Id"].ToString();
            return Guid.TryParse(headerValue, out var workspaceId) ? workspaceId : null;
        }
    }
}
