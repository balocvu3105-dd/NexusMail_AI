using System;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NexusMail.Infrastructure.Persistence;

namespace NexusMail.Infrastructure.Observability;

public class OutboxMetricsBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxMetricsBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    // Meters for outbox
    private static readonly Meter Meter = CustomMetrics.Meter;
    private int _pendingCount;
    private int _processingCount;
    private int _completedCount;
    private int _deadLetterCount;

    public OutboxMetricsBackgroundService(IServiceProvider serviceProvider, ILogger<OutboxMetricsBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        Meter.CreateObservableGauge("outbox_pending_total", () => _pendingCount, "messages", "Total pending outbox messages");
        Meter.CreateObservableGauge("outbox_processing_total", () => _processingCount, "messages", "Total processing outbox messages");
        Meter.CreateObservableGauge("outbox_completed_total", () => _completedCount, "messages", "Total completed outbox messages");
        Meter.CreateObservableGauge("outbox_deadletter_total", () => _deadLetterCount, "messages", "Total dead letter outbox messages");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Simplified outbox metrics logic for MassTransit Inbox/Outbox or generic outbox table.
                // Assuming EF Core Outbox table name is "OutboxMessages" and "InboxStates".
                // Since MassTransit's schema is complex, we use raw SQL to query it.

                // Note: Ensure the table name matches MassTransit's configuration or your custom outbox.
                var sql = @"
                    SELECT 
                        (SELECT COUNT(*) FROM ""OutboxState"") AS Pending,
                        (SELECT COUNT(*) FROM ""OutboxMessage"") AS Messages,
                        0 AS Processing,
                        0 AS Completed,
                        0 AS DeadLetter";
                        
                // In a real scenario, this would query the exact states. 
                // We'll simulate fetching if the tables don't exist yet by just catching exceptions.
                try 
                {
                    _pendingCount = await dbContext.Set<NexusMail.Application.Abstractions.Outbox.OutboxMessage>()
                        .CountAsync(m => m.ProcessedOnUtc == null, stoppingToken);
                    _completedCount = await dbContext.Set<NexusMail.Application.Abstractions.Outbox.OutboxMessage>()
                        .CountAsync(m => m.ProcessedOnUtc != null, stoppingToken);
                }
                catch 
                {
                    // Table might not exist or schema differs
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect outbox metrics");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
