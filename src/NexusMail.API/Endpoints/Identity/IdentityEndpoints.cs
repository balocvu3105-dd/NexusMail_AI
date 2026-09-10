using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace NexusMail.API.Endpoints.Identity;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/identity")
            .WithTags("Identity")
            .HasApiVersion(1.0);

        group.MapPost("register", async ([Microsoft.AspNetCore.Mvc.FromBody] NexusMail.Application.Features.Identity.Commands.Register.RegisterCommand command, MediatR.IMediator mediator) => 
        {
            var result = await mediator.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost("login", async ([Microsoft.AspNetCore.Mvc.FromBody] NexusMail.Application.Features.Identity.Commands.Login.LoginCommand command, MediatR.IMediator mediator) => 
        {
            var result = await mediator.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireRateLimiting("LoginPolicy");

        group.MapPost("refresh-token", async ([Microsoft.AspNetCore.Mvc.FromBody] NexusMail.Application.Features.Identity.Commands.RefreshToken.RefreshTokenCommand command, MediatR.IMediator mediator) => 
        {
            var result = await mediator.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireRateLimiting("RefreshPolicy");

        group.MapPost("logout", async ([Microsoft.AspNetCore.Mvc.FromBody] NexusMail.Application.Features.Identity.Commands.Logout.LogoutCommand command, MediatR.IMediator mediator) => 
        {
            var result = await mediator.Send(command);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });
    }
}
