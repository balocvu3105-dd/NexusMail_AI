using Microsoft.Extensions.Logging;
using NexusMail.Common.Constants;

namespace NexusMail.Application.Features.Email.Pipeline.Steps;

/// <summary>
/// Step 3 — Rate limit enforcement for email provider API quotas.
///
/// WHY AS A PIPELINE STEP:
///   Rate limiting should NOT be inside GoogleEmailProvider because:
///   - Different providers have different quota models
///   - Rate limit state may need to be shared across parallel sync jobs
///   - It's a cross-cutting infrastructure concern, not a transport concern
///
/// Gmail API quotas (as of 2024):
///   - 250 quota units / user / second
///   - GetMessages: 5 units per call
///   - GetMessage (full): 5 units per call
///
/// TODO Sprint 4:
///   - Integrate with Redis sliding window rate limiter for distributed scenarios
///   - Read Retry-After header from provider responses
/// </summary>
public sealed class RateLimitStep : ISyncPipelineStep
{
    private readonly ILogger<RateLimitStep> _logger;

    // Conservative default: max 20 API calls per sync run before throttling
    public const int MaxCallsPerRun = 20;
    public static readonly TimeSpan ThrottleDelay = TimeSpan.FromMilliseconds(100);

    public string StepName => "RateLimit";

    public RateLimitStep(ILogger<RateLimitStep> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(SyncContext context, CancellationToken cancellationToken = default)
    {
        var messageCount = context.FetchedMessageIds.Count;

        if (messageCount > NexusConstants.Email.SyncBatchSize)
        {
            _logger.LogWarning(
                "[{Step}] Large batch detected ({Count} messages). Applying throttle delay to respect API quotas.",
                StepName, messageCount);

            context.Result.RateLimitHits++;
            await Task.Delay(ThrottleDelay, cancellationToken);
        }
        else
        {
            _logger.LogDebug(
                "[{Step}] Batch size {Count} within quota limits. No throttle needed.",
                StepName, messageCount);
        }
    }
}
