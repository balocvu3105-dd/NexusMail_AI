using System;
using Microsoft.AspNetCore.Http;
using NexusMail.API.Models;
using NexusMail.Application.Abstractions.Time;
using NexusMail.Shared.Common;

namespace NexusMail.API.Infrastructure.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result, HttpContext context, IClock clock)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(new 
            {
                traceId = context.TraceIdentifier,
                timestamp = clock.UtcNow
            });
        }
        
        return Results.BadRequest(new { error = result.Error });
    }

    public static IResult ToHttpResult<T>(this Result<T> result, HttpContext context, IClock clock)
    {
        if (result.IsSuccess)
        {
            var response = new ApiResponse<T>(
                result.Value,
                context.TraceIdentifier,
                clock.UtcNow
            );
            return Results.Ok(response);
        }
        
        return Results.BadRequest(new { error = result.Error });
    }

    public static IResult ToCreatedResult<T>(this Result<T> result, string uri, HttpContext context, IClock clock)
    {
        if (result.IsSuccess)
        {
            var response = new ApiResponse<T>(
                result.Value,
                context.TraceIdentifier,
                clock.UtcNow
            );
            return Results.Created(uri, response);
        }
        
        return Results.BadRequest(new { error = result.Error });
    }
}
