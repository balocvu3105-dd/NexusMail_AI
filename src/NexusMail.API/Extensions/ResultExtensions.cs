using Microsoft.AspNetCore.Http;
using NexusMail.Shared.Common;
using System.Collections.Generic;

namespace NexusMail.API.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new System.InvalidOperationException("Cannot convert a successful result to ProblemDetails.");
        }

        return Results.Problem(
            statusCode: GetStatusCode(result.Error),
            title: GetTitle(result.Error),
            type: GetType(result.Error),
            extensions: new Dictionary<string, object?>
            {
                { "errors", new[] { result.Error } }
            });
    }

    private static int GetStatusCode(Error error) =>
        error.Code switch
        {
            var code when code.Contains("NotFound", System.StringComparison.OrdinalIgnoreCase) => StatusCodes.Status404NotFound,
            var code when code.Contains("Unauthorized", System.StringComparison.OrdinalIgnoreCase) => StatusCodes.Status401Unauthorized,
            var code when code.Contains("Forbidden", System.StringComparison.OrdinalIgnoreCase) => StatusCodes.Status403Forbidden,
            var code when code.Contains("Conflict", System.StringComparison.OrdinalIgnoreCase) => StatusCodes.Status409Conflict,
            var code when code.Contains("Exists", System.StringComparison.OrdinalIgnoreCase) => StatusCodes.Status409Conflict,
            var code when code.Contains("Concurrency", System.StringComparison.OrdinalIgnoreCase) => StatusCodes.Status409Conflict,
            var code when code.Contains("Validation", System.StringComparison.OrdinalIgnoreCase) => StatusCodes.Status400BadRequest,
            var code when code.Contains("BusinessRule", System.StringComparison.OrdinalIgnoreCase) => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };

    private static string GetTitle(Error error) =>
        error.Code switch
        {
            var code when code.Contains("NotFound", System.StringComparison.OrdinalIgnoreCase) => "Not Found",
            var code when code.Contains("Unauthorized", System.StringComparison.OrdinalIgnoreCase) => "Unauthorized",
            var code when code.Contains("Forbidden", System.StringComparison.OrdinalIgnoreCase) => "Forbidden",
            var code when code.Contains("Conflict", System.StringComparison.OrdinalIgnoreCase) => "Conflict",
            var code when code.Contains("Exists", System.StringComparison.OrdinalIgnoreCase) => "Conflict",
            var code when code.Contains("Concurrency", System.StringComparison.OrdinalIgnoreCase) => "Concurrency Error",
            var code when code.Contains("Validation", System.StringComparison.OrdinalIgnoreCase) => "Validation Error",
            var code when code.Contains("BusinessRule", System.StringComparison.OrdinalIgnoreCase) => "Unprocessable Entity",
            _ => "Bad Request"
        };

    private static string GetType(Error error) =>
        error.Code switch
        {
            var code when code.Contains("NotFound", System.StringComparison.OrdinalIgnoreCase) => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            var code when code.Contains("Unauthorized", System.StringComparison.OrdinalIgnoreCase) => "https://tools.ietf.org/html/rfc7235#section-3.1",
            var code when code.Contains("Forbidden", System.StringComparison.OrdinalIgnoreCase) => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            var code when code.Contains("Conflict", System.StringComparison.OrdinalIgnoreCase) => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            var code when code.Contains("Exists", System.StringComparison.OrdinalIgnoreCase) => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            var code when code.Contains("Concurrency", System.StringComparison.OrdinalIgnoreCase) => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            var code when code.Contains("Validation", System.StringComparison.OrdinalIgnoreCase) => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            var code when code.Contains("BusinessRule", System.StringComparison.OrdinalIgnoreCase) => "https://tools.ietf.org/html/rfc4918#section-11.2",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };
}
