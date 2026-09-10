using Microsoft.Extensions.Logging;
using NexusMail.Application.Features.Email.Pipeline.Models;

namespace NexusMail.Application.Features.Email.Pipeline;

/// <summary>
/// Orchestrates the email synchronization pipeline.
/// Runs steps sequentially and stops on ShouldAbort or cancellation.
///
/// Steps (in order):
///   1. FetchEmailsStep
///   2. RetryStep
///   3. RateLimitStep
///   4. ValidateEmailsStep
///   5. NormalizeEmailsStep
///   6. DeduplicateEmailsStep
///   7. PersistEmailsStep
///   8. PublishDomainEventsStep
///   9. RecordMetricsStep
/// </summary>
public sealed class EmailSyncPipeline
{
    private readonly IReadOnlyList<ISyncPipelineStep> _steps;
    private readonly ILogger<EmailSyncPipeline> _logger;

    public EmailSyncPipeline(
        IEnumerable<ISyncPipelineStep> steps,
        ILogger<EmailSyncPipeline> logger)
    {
        _steps = steps.ToList().AsReadOnly();
        _logger = logger;
    }

    public async Task<SyncResult> RunAsync(SyncContext context, CancellationToken cancellationToken = default)
    {
        var startTime = DateTimeOffset.UtcNow;
        _logger.LogInformation(
            "Starting email sync pipeline for account {AccountId} [CorrelationId: {CorrelationId}]",
            context.EmailAccountId, context.CorrelationId);

        foreach (var step in _steps)
        {
            if (context.ShouldAbort)
            {
                _logger.LogWarning(
                    "Pipeline aborted before step [{Step}]. Reason: {Reason}",
                    step.StepName, context.AbortReason);
                break;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Pipeline cancelled at step [{Step}].", step.StepName);
                context.Abort("CancellationToken requested.");
                break;
            }

            try
            {
                _logger.LogDebug("Executing pipeline step [{Step}]", step.StepName);
                await step.ExecuteAsync(context, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in pipeline step [{Step}].", step.StepName);
                context.Abort($"Step [{step.StepName}] threw: {ex.Message}");
                break;
            }
        }

        var result = context.Result;
        result.Duration = DateTimeOffset.UtcNow - startTime;
        result.WasAborted = context.ShouldAbort;
        result.AbortReason = context.AbortReason;

        _logger.LogInformation(
            "Email sync pipeline completed for account {AccountId}. {Result}",
            context.EmailAccountId, result);

        return result;
    }
}
