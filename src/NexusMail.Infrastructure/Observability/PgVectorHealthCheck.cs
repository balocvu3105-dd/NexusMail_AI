using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using NexusMail.Infrastructure.Persistence;

namespace NexusMail.Infrastructure.Observability;

public class PgVectorHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PgVectorHealthCheck> _logger;

    public PgVectorHealthCheck(ApplicationDbContext dbContext, ILogger<PgVectorHealthCheck> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple query to verify pgvector extension is available and functional
            var isPgVectorAvailable = await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT 1 FROM pg_extension WHERE extname = 'vector'", cancellationToken);
                
            return HealthCheckResult.Healthy("pgvector extension is available");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "pgvector HealthCheck failed");
            return HealthCheckResult.Unhealthy("pgvector extension is missing or broken", ex);
        }
    }
}
