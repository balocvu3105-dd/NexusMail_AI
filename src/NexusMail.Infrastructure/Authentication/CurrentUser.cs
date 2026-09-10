using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NexusMail.Application.Abstractions.Authentication;

namespace NexusMail.Infrastructure.Authentication;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
                           ?? _httpContextAccessor.HttpContext?.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public Guid? WorkspaceId
    {
        get
        {
            var workspaceIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("workspace_id")?.Value;
            return Guid.TryParse(workspaceIdClaim, out var workspaceId) ? workspaceId : null;
        }
    }

    public bool IsAuthenticated => UserId.HasValue;
}
