using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using NexusMail.Application.Features.Email.Commands.ConnectEmailAccount;
using NexusMail.Application.Features.Email.Commands.UpdateEmailAccountSettings;
using NexusMail.Application.Features.Email.Commands.DisconnectEmailAccount;
using NexusMail.Application.Features.Email.Queries.GetEmailAccounts;
using NexusMail.API.Extensions;
using NexusMail.Shared.Pagination;

namespace NexusMail.API.Endpoints.Email;

public static class EmailEndpoints
{
    public static void MapEmailEndpoints(this IEndpointRouteBuilder app)
    {
        var accountGroup = app.MapGroup("api/v{version:apiVersion}/email-accounts")
            .WithTags("Email Accounts")
            .HasApiVersion(1.0)
            .RequireAuthorization();

        accountGroup.MapPost("", async (
            ConnectEmailAccountCommand command,
            MediatR.ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess 
                ? Results.Created($"/api/v1/email-accounts/{result.Value}", result.Value) 
                : result.ToProblemDetails();
        });

        accountGroup.MapGet("", async (
            [Microsoft.AspNetCore.Mvc.FromQuery] int? page,
            [Microsoft.AspNetCore.Mvc.FromQuery] int? pageSize,
            MediatR.ISender sender) =>
        {
            var query = new GetEmailAccountsQuery(new PageRequest(page > 0 ? page.Value : 1, pageSize > 0 ? pageSize.Value : 20));
            var result = await sender.Send(query);
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : result.ToProblemDetails();
        });

        accountGroup.MapPatch("{id:guid}", async (
            System.Guid id,
            UpdateEmailAccountSettingsCommand command,
            MediatR.ISender sender) =>
        {
            if (id != command.Id) return Results.BadRequest();
            var result = await sender.Send(command);
            return result.IsSuccess 
                ? Results.NoContent() 
                : result.ToProblemDetails();
        });

        accountGroup.MapDelete("{id:guid}", async (
            System.Guid id,
            MediatR.ISender sender) =>
        {
            var result = await sender.Send(new DisconnectEmailAccountCommand(id));
            return result.IsSuccess 
                ? Results.NoContent() 
                : result.ToProblemDetails();
        });

        var group = app.MapGroup("api/v{version:apiVersion}/emails")
            .WithTags("Emails")
            .HasApiVersion(1.0);

        group.MapPost("connect", async (MediatR.ISender sender) =>
        {
            var command = new ConnectEmailAccountCommand(
                NexusMail.Domain.Email.Enums.EmailProvider.Fake,
                "dummy_auth_code",
                "http://localhost"
            );
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
        });
        group.MapGet("inbox", async (
            MediatR.ISender sender,
            NexusMail.Application.Abstractions.Authentication.IWorkspaceContext workspaceContext) =>
        {
            if (workspaceContext.WorkspaceId == null) return Results.BadRequest("Missing X-Workspace-Id header");
            var result = await sender.Send(new NexusMail.Application.Features.Email.Queries.GetInbox.GetInboxQuery(workspaceContext.WorkspaceId.Value));
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : result.ToProblemDetails();
        });
        group.MapGet("status", async (
            MediatR.ISender sender,
            NexusMail.Application.Abstractions.Authentication.IWorkspaceContext workspaceContext) =>
        {
            if (workspaceContext.WorkspaceId == null) return Results.BadRequest("Missing X-Workspace-Id header");
            var result = await sender.Send(new NexusMail.Application.Features.Email.Queries.GetInboxStatus.GetInboxStatusQuery(workspaceContext.WorkspaceId.Value));
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : result.ToProblemDetails();
        });
        group.MapGet("search", async (
            [Microsoft.AspNetCore.Mvc.FromQuery] string query,
            [Microsoft.AspNetCore.Mvc.FromQuery] int? page,
            [Microsoft.AspNetCore.Mvc.FromQuery] int? pageSize,
            MediatR.ISender sender,
            NexusMail.Application.Abstractions.Authentication.IWorkspaceContext workspaceContext) =>
        {
            if (string.IsNullOrWhiteSpace(query)) return Results.BadRequest("Query text is required.");
            var searchCommand = new NexusMail.Application.Features.Email.Queries.SearchEmails.SearchEmailsQuery(
                query,
                Page: page ?? 1,
                PageSize: pageSize ?? 20
            );
            var result = await sender.Send(searchCommand);
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : result.ToProblemDetails();
        });

        group.MapGet("{id:guid}", async (
            System.Guid id,
            MediatR.ISender sender,
            NexusMail.Application.Abstractions.Authentication.IWorkspaceContext workspaceContext) =>
        {
            var result = await sender.Send(new NexusMail.Application.Features.Email.Queries.GetEmail.GetEmailQuery(id));
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : result.ToProblemDetails();
        });
    }
}
