using Microsoft.Extensions.DependencyInjection;
using NexusMail.Application.Features.Email.Abstractions;

namespace NexusMail.Worker.EmailSync;

public class Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("EmailSync Worker started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IEmailAccountRepository>();
                var syncService = scope.ServiceProvider.GetRequiredService<IEmailSynchronizationService>();

                // 1. Query all active email accounts due for sync
                var accounts = await repository.GetAccountsDueForSyncAsync(10, stoppingToken);

                foreach (var account in accounts)
                {
                    // Basic error isolation — if one account fails, we still process the rest
                    try
                    {
                        var correlationId = Guid.NewGuid().ToString("N");
                        await syncService.SyncEmailAccountAsync(account.Id, correlationId, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to sync account {AccountId}", account.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in EmailSync Worker loop");
            }

            // Polling interval
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
