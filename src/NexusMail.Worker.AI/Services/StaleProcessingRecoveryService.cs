using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Abstractions.AI;
using NexusMail.Contracts.AI;
using NexusMail.Domain.AI.Enums;
using NexusMail.Infrastructure.Persistence;

namespace NexusMail.Worker.AI.Services;

public class StaleProcessingRecoveryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StaleProcessingRecoveryService> _logger;

    public StaleProcessingRecoveryService(IServiceProvider serviceProvider, ILogger<StaleProcessingRecoveryService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StaleProcessingRecoveryService started.");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var bus = scope.ServiceProvider.GetRequiredService<IBus>();

                // Any Processing state older than 6 minutes is considered stale (giving 5min delayed redelivery a chance)
                var cutoff = DateTimeOffset.UtcNow.AddMinutes(-6);
                var staleRecords = await db.AIAnalyses
                    .Where(a => a.ProcessingState == AIProcessingStatus.Processing && a.ProcessingStartedAt < cutoff)
                    .ToListAsync(stoppingToken);

                foreach (var record in staleRecords)
                {
                    _logger.LogWarning("Recovering stale AIAnalysis for EmailId: {EmailId}", record.EmailId);
                    
                    if (record.ProcessingAttemptId.HasValue)
                    {
                        record.ResetProcessingToPending(record.ProcessingAttemptId.Value);
                    }
                    
                    await db.SaveChangesAsync(stoppingToken);
                    await bus.Publish(new AIProcessingRequestedMessage { EmailId = record.EmailId }, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during stale recovery sweep.");
            }

            // Run every 2 minutes
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}
