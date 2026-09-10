using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace NexusMail.API.Endpoints.AI;

public static class AIEndpoints
{
    public static void MapAIEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/ai")
            .WithTags("AI")
            .HasApiVersion(1.0);

        group.MapPost("generate-summary", () => Results.Ok());
        group.MapPost("generate-reply", () => Results.Ok());
    }
}
