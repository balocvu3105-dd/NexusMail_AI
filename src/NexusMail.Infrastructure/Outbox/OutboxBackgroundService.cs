using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Abstractions.Outbox;

namespace NexusMail.Infrastructure.Outbox;

public sealed class OutboxBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxBackgroundService> _logger;
    private readonly OutboxOptions _options;

    public OutboxBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<OutboxBackgroundService> logger,
        Microsoft.Extensions.Options.IOptions<OutboxOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<IOutboxDispatcher>();
                
                await dispatcher.DispatchUnprocessedMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while dispatching outbox messages.");
            }

            await Task.Delay(_options.PollInterval, stoppingToken);
        }
        
        _logger.LogInformation("Outbox Background Service is stopping.");
    }
}
