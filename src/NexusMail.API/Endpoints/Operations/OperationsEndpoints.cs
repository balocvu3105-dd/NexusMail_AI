using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NexusMail.Infrastructure.Persistence;

namespace NexusMail.API.Endpoints.Operations;

public static class OperationsEndpoints
{
    public static RouteGroupBuilder MapOperationsEndpoints(this RouteGroupBuilder group)
    {
        var operationsGroup = group.MapGroup("/operations").WithTags("Operations");

        operationsGroup.MapGet("/outbox", async (ApplicationDbContext dbContext) =>
        {
            try
            {
                // Simplified outbox metrics logic for MassTransit Inbox/Outbox
                var pendingCount = await dbContext.Database.SqlQueryRaw<int>(@"SELECT COUNT(*) FROM ""OutboxMessage"" WHERE ""Delivered"" IS NULL").FirstOrDefaultAsync();
                var completedCount = await dbContext.Database.SqlQueryRaw<int>(@"SELECT COUNT(*) FROM ""OutboxMessage"" WHERE ""Delivered"" IS NOT NULL").FirstOrDefaultAsync();
                
                return Results.Ok(new
                {
                    Pending = pendingCount,
                    Processing = 0,
                    Completed = completedCount,
                    DeadLetter = 0
                });
            }
            catch
            {
                return Results.Ok(new
                {
                    Pending = 0,
                    Processing = 0,
                    Completed = 0,
                    DeadLetter = 0
                });
            }
        })
        .WithName("GetOutboxStats")
        .WithDescription("Gets the statistics of the outbox messages");

        return group;
    }
}
