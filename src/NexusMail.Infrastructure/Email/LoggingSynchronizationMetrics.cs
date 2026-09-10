using System;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Features.Email.Abstractions;

namespace NexusMail.Infrastructure.Email;

public class LoggingSynchronizationMetrics : ISynchronizationMetrics
{
    private readonly ILogger<LoggingSynchronizationMetrics> _logger;
    private int _runningWorkers;

    public LoggingSynchronizationMetrics(ILogger<LoggingSynchronizationMetrics> logger)
    {
        _logger = logger;
    }

    public void IncrementEmailsSynced(int count = 1)
    {
        _logger.LogInformation("METRIC [Counter] EmailsSynced += {Count}", count);
    }

    public void IncrementRetryCount()
    {
        _logger.LogInformation("METRIC [Counter] RetryCount += 1");
    }

    public void IncrementFailedSyncs()
    {
        _logger.LogInformation("METRIC [Counter] FailedSyncs += 1");
    }

    public void RecordSyncDuration(TimeSpan duration)
    {
        _logger.LogInformation("METRIC [Histogram] SyncDuration = {DurationMs}ms", duration.TotalMilliseconds);
    }

    public void IncrementRunningWorkers()
    {
        var current = System.Threading.Interlocked.Increment(ref _runningWorkers);
        _logger.LogInformation("METRIC [Gauge] RunningWorkers = {Count}", current);
    }

    public void DecrementRunningWorkers()
    {
        var current = System.Threading.Interlocked.Decrement(ref _runningWorkers);
        _logger.LogInformation("METRIC [Gauge] RunningWorkers = {Count}", current);
    }
}
