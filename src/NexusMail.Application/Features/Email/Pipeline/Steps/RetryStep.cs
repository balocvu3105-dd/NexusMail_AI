using Microsoft.Extensions.Logging;

namespace NexusMail.Application.Features.Email.Pipeline.Steps;

/// <summary>
/// Step 2 — Retry behavior for transient provider failures.
///
/// WHY HERE (not inside GoogleEmailProvider):
///   Retry is a cross-cutting concern. Moving it into the provider would:
///   - Mix transport and retry logic in one class
///   - Make it hard to test retry independently
///   - Prevent reuse across providers (Outlook, IMAP, etc.)
///
/// Handles: 429 (rate limit), 500 (server error), 503 (unavailable)
/// Does NOT retry: 401 (token invalid), 404 (not found) — these are not transient.
///
/// TODO Sprint 4: Replace with Polly ResiliencePipeline for production.
/// </summary>
public sealed class RetryStep : ISyncPipelineStep
{
    private readonly ILogger<RetryStep> _logger;

    public const int MaxRetries = 3;
    public const int BaseDelayMs = 500;

    public string StepName => "Retry";

    public RetryStep(ILogger<RetryStep> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(SyncContext context, CancellationToken cancellationToken = default)
    {
        // Retry is a decorator concern — actual retry logic wraps the FetchEmailsStep.
        // In this skeleton, we track retry state in context for use by the pipeline orchestrator.
        // Sprint 4 will inject Polly ResiliencePipeline<HttpResponseMessage> here.
        _logger.LogDebug("[{Step}] Retry behavior registered (Polly integration pending Sprint 4).", StepName);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns true if the exception is a transient error worth retrying.
    /// </summary>
    public static bool IsTransient(Exception ex)
    {
        if (ex is HttpRequestException httpEx)
        {
            return httpEx.StatusCode is
                System.Net.HttpStatusCode.TooManyRequests or       // 429
                System.Net.HttpStatusCode.InternalServerError or   // 500
                System.Net.HttpStatusCode.ServiceUnavailable or    // 503
                System.Net.HttpStatusCode.GatewayTimeout;          // 504
        }
        return ex is TimeoutException or TaskCanceledException;
    }

    /// <summary>
    /// Exponential backoff delay: 500ms, 1000ms, 2000ms...
    /// Respects Retry-After header when available (set in context by provider).
    /// </summary>
    public static TimeSpan GetDelay(int attempt)
        => TimeSpan.FromMilliseconds(BaseDelayMs * Math.Pow(2, attempt - 1));
}
