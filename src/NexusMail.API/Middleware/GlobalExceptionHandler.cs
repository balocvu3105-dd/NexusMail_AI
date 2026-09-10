using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Common.Exceptions;

namespace NexusMail.API.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception, "Exception occurred: {Message}", exception.Message);

        var statusCode = GetStatusCode(exception);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Type = exception.GetType().Name,
            Title = GetTitle(exception),
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors;
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static int GetStatusCode(Exception exception) =>
        exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

    private static string GetTitle(Exception exception) =>
        exception switch
        {
            ValidationException => "Validation Error",
            UnauthorizedAccessException => "Unauthorized",
            _ => "Server Error"
        };
}
