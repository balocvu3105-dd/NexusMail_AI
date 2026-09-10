using Microsoft.Extensions.Logging;
using NexusMail.Application.Features.Email.Abstractions;

namespace NexusMail.Application.Features.Email.Pipeline.Steps;

/// <summary>
/// Step 9 — Record sync metrics (final step).
/// Writes counters and histograms to ISynchronizationMetrics.
/// This step NEVER throws — metric failures must not abort the pipeline.
/// </summary>
public sealed class RecordMetricsStep : ISyncPipelineStep
{
    private readonly ISynchronizationMetrics _metrics;
    private readonly ILogger<RecordMetricsStep> _logger;

    public string StepName => "RecordMetrics";

    public RecordMetricsStep(ISynchronizationMetrics metrics, ILogger<RecordMetricsStep> logger)
    {
        _metrics = metrics;
        _logger = logger;
    }

    public Task ExecuteAsync(SyncContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = context.Result;
            _metrics.IncrementEmailsSynced(result.Persisted);
            _metrics.RecordSyncDuration(result.Duration);

            if (result.RetryCount > 0)
                _metrics.IncrementRetryCount();

            if (result.WasAborted)
                _metrics.IncrementFailedSyncs();

            _logger.LogInformation(
                "[{Step}] Sync summary for account {AccountId}: {Result}",
                StepName, context.EmailAccountId, result);
        }
        catch (Exception ex)
        {
            // Metric failures must never propagate
            _logger.LogError(ex, "[{Step}] Failed to record metrics. Ignoring.", StepName);
        }

        return Task.CompletedTask;
    }
}
