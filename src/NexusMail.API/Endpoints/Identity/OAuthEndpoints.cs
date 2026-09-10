using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Application.Features.Email.Commands.ConnectEmailAccount;
using NexusMail.Domain.Email.Enums;
using NexusMail.Infrastructure.Authentication;
using NexusMail.API.Extensions;
using MediatR;
using System.Linq;

namespace NexusMail.API.Endpoints.Identity;

public static class OAuthEndpoints
{
    private const string StateProtectorPurpose = "NexusMail.OAuth.State";

    public static void MapOAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/auth/google")
            .WithTags("OAuth")
            .HasApiVersion(1.0);

        group.MapGet("login", (
            [FromQuery] Guid workspaceId,
            HttpContext httpContext,
            ICurrentUser currentUser,
            IDataProtectionProvider dataProtector,
            IOptions<GoogleOAuthOptions> options) =>
        {
            if (currentUser.UserId == null || currentUser.UserId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var csrfToken = Guid.NewGuid().ToString("N");
            httpContext.Response.Cookies.Append("oauth_csrf", csrfToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromMinutes(10)
            });

            var payload = new OAuthStatePayload(
                UserId: currentUser.UserId.Value,
                WorkspaceId: workspaceId,
                ExpiresAt: DateTimeOffset.UtcNow.AddMinutes(10),
                CsrfToken: csrfToken
            );

            var protector = dataProtector.CreateProtector(StateProtectorPurpose);
            var stateJson = JsonSerializer.Serialize(payload);
            var encryptedState = protector.Protect(stateJson);

            var redirectUri = options.Value.RedirectUri;
            var scopes = "https://www.googleapis.com/auth/gmail.readonly https://www.googleapis.com/auth/userinfo.email";
            
            var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth" +
                          $"?client_id={options.Value.ClientId}" +
                          $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                          $"&response_type=code" +
                          $"&scope={Uri.EscapeDataString(scopes)}" +
                          $"&access_type=offline" +
                          $"&prompt=consent" +
                          $"&state={Uri.EscapeDataString(encryptedState)}";

            return Results.Redirect(authUrl);
        }).RequireAuthorization();

        group.MapGet("callback", async (
            [FromQuery] string? code,
            [FromQuery] string? state,
            [FromQuery] string? error,
            HttpContext httpContext,
            IDataProtectionProvider dataProtector,
            ISender sender,
            NexusMail.Domain.Workspace.Repositories.IWorkspaceRepository workspaceRepository,
            IOptions<GoogleOAuthOptions> options) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                return Results.Redirect($"/settings/accounts?connected=google&status=error&error={Uri.EscapeDataString(error)}");
            }

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                return Results.Redirect("/settings/accounts?connected=google&status=error&error=missing_code_or_state");
            }

            // 1. Validate State
            var protector = dataProtector.CreateProtector(StateProtectorPurpose);
            string stateJson;
            try
            {
                stateJson = protector.Unprotect(state);
            }
            catch
            {
                return Results.Redirect("/settings/accounts?connected=google&status=error&error=invalid_state");
            }

            var payload = JsonSerializer.Deserialize<OAuthStatePayload>(stateJson);
            if (payload == null)
            {
                return Results.Redirect("/settings/accounts?connected=google&status=error&error=invalid_state_payload");
            }

            if (payload.ExpiresAt < DateTimeOffset.UtcNow)
            {
                return Results.Redirect("/settings/accounts?connected=google&status=error&error=state_expired");
            }

            // CSRF validation
            if (!httpContext.Request.Cookies.TryGetValue("oauth_csrf", out var cookieCsrf) || cookieCsrf != payload.CsrfToken)
            {
                return Results.Redirect("/settings/accounts?connected=google&status=error&error=csrf_failed");
            }
            httpContext.Response.Cookies.Delete("oauth_csrf");

            // 2. Authorize Workspace
            // Ensure this user is actually a member of the workspace requested in the state
            var isMember = await workspaceRepository.ExistsMemberAsync(payload.WorkspaceId, payload.UserId, httpContext.RequestAborted);
            if (!isMember)
            {
                return Results.Redirect("/settings/accounts?connected=google&status=error&error=unauthorized_workspace");
            }

            // Inject the X-Workspace-Id header so IWorkspaceContext correctly resolves it for the Command Handler
            httpContext.Request.Headers["X-Workspace-Id"] = payload.WorkspaceId.ToString();

            // 3. Connect Account
            var command = new ConnectEmailAccountCommand(
                EmailProvider.Google,
                code,
                options.Value.RedirectUri
            );

            var result = await sender.Send(command);

            if (result.IsFailure)
            {
                return Results.Redirect($"/settings/accounts?connected=google&status=error&error={Uri.EscapeDataString(result.Error.Code)}");
            }

            return Results.Redirect("/settings/accounts?connected=google&status=success");
        });
    }

    private record OAuthStatePayload(Guid UserId, Guid WorkspaceId, DateTimeOffset ExpiresAt, string CsrfToken);
}
