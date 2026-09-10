using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace NexusMail.API.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, NexusMail.Application.Abstractions.Context.IExecutionContextAccessor executionContextAccessor)
    {
        var correlationId = GetCorrelationId(context);
        
        // Set TraceIdentifier so it flows everywhere
        context.TraceIdentifier = correlationId;
        
        AddCorrelationIdHeaderToResponse(context, correlationId);

        // Update Execution Context
        var executionContext = new NexusMail.Application.Abstractions.Context.ExecutionContext { CorrelationId = correlationId };
        executionContextAccessor.SetContext(executionContext);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out StringValues correlationId))
        {
            return correlationId.ToString();
        }
        return Guid.NewGuid().ToString();
    }

    private static void AddCorrelationIdHeaderToResponse(HttpContext context, string correlationId)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = new[] { correlationId };
            return Task.CompletedTask;
        });
    }
}
