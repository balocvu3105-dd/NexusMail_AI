using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NexusMail.API.Extensions;
using NexusMail.Application.Features.Notification.Commands;
using NexusMail.Application.Features.Notification.Queries;
using System;
using System.Threading;

namespace NexusMail.API.Endpoints.Notification;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/v1/notifications")
            .RequireAuthorization()
            .AddEndpointFilter<NexusMail.API.Middleware.WorkspaceContextEndpointFilter>()
            .WithTags("Notifications");

        group.MapGet("", async (
            ISender sender,
            bool unreadOnly = false,
            int limit = 20,
            CancellationToken cancellationToken = default) =>
        {
            var query = new GetNotificationsQuery(unreadOnly, limit);
            var result = await sender.Send(query, cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
        });

        group.MapPatch("{id:guid}/read", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            var command = new MarkAsReadCommand(id);
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
        });

        group.MapPost("read-all", async (
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            var command = new MarkAllAsReadCommand();
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
        });
    }
}
