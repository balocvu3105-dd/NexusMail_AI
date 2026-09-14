using System;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Application.Abstractions.Copilot;
using NexusMail.API.Extensions;

namespace NexusMail.API.Endpoints.Copilot;

public sealed record AskCopilotRequest(string Query);

public static class CopilotEndpoints
{
    public static void MapCopilotEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/copilot")
            .WithTags("Copilot")
            .HasApiVersion(1.0)
            .RequireAuthorization();

        group.MapPost("ask", async (
            [FromBody] AskCopilotRequest request,
            ICopilotOrchestrator orchestrator,
            IWorkspaceContext workspaceContext,
            CancellationToken ct) =>
        {
            if (workspaceContext.WorkspaceId == null)
                return Results.BadRequest("Missing X-Workspace-Id header");

            if (string.IsNullOrWhiteSpace(request.Query))
                return Results.BadRequest("Query is required");

            var query = new CopilotQuery
            {
                WorkspaceId = workspaceContext.WorkspaceId.Value,
                QueryText = request.Query
            };

            var response = await orchestrator.AskAsync(query, ct);
            return Results.Ok(response);
        });

        group.MapPost("stream", async (
            [FromBody] AskCopilotRequest request,
            ICopilotOrchestrator orchestrator,
            IWorkspaceContext workspaceContext,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            if (workspaceContext.WorkspaceId == null)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("Missing X-Workspace-Id header");
                return;
            }

            if (string.IsNullOrWhiteSpace(request.Query))
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("Query is required");
                return;
            }

            httpContext.Response.Headers.Append("Content-Type", "text/event-stream");
            httpContext.Response.Headers.Append("Cache-Control", "no-cache");
            httpContext.Response.Headers.Append("Connection", "keep-alive");

            var query = new CopilotQuery
            {
                WorkspaceId = workspaceContext.WorkspaceId.Value,
                QueryText = request.Query
            };

            var stream = orchestrator.AskStreamAsync(query, ct);

            await foreach (var chunk in stream)
            {
                if (!chunk.IsDone)
                {
                    // text event
                    var data = new { text = chunk.ChunkText };
                    var json = JsonSerializer.Serialize(data);
                    await httpContext.Response.WriteAsync($"event: chunk\ndata: {json}\n\n", ct);
                    await httpContext.Response.Body.FlushAsync(ct);
                }
                else
                {
                    // complete event
                    var data = new { 
                        state = chunk.State?.ToString(), 
                        evidence = chunk.Evidence,
                        suggestedActions = chunk.SuggestedActions
                    };
                    var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    await httpContext.Response.WriteAsync($"event: complete\ndata: {json}\n\n", ct);
                    await httpContext.Response.Body.FlushAsync(ct);
                }
            }
        });

        group.MapPost("actions/execute", async (
            [FromBody] ExecuteActionDto request,
            ICopilotActionService actionService,
            IWorkspaceContext workspaceContext,
            CancellationToken ct) =>
        {
            if (workspaceContext.WorkspaceId == null)
                return Results.BadRequest("Missing X-Workspace-Id header");

            if (request.Proposal == null)
                return Results.BadRequest("Proposal is required");

            if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
                return Results.BadRequest("IdempotencyKey is required");

            var execRequest = new ActionExecutionRequest
            {
                WorkspaceId = workspaceContext.WorkspaceId.Value,
                IdempotencyKey = request.IdempotencyKey,
                Proposal = request.Proposal
            };

            var result = await actionService.ExecuteActionAsync(execRequest, ct);

            if (result.IsSuccess)
            {
                return Results.Ok();
            }
            
            return Results.BadRequest(result.Error);
        });
    }
}

public sealed record ExecuteActionDto(string IdempotencyKey, ActionProposal Proposal);
