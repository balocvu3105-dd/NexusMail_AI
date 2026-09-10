namespace NexusMail.Application.Features.Email.Pipeline;

/// <summary>
/// A single step in the email synchronization pipeline.
///
/// PIPELINE ORDER:
///   1. FetchEmailsStep      — call email provider API
///   2. RetryStep            — retry failed API calls (429, 500, 503)
///   3. RateLimitStep        — throttle to respect provider quotas
///   4. ValidateEmailsStep   — filter malformed or incomplete messages
///   5. NormalizeEmailsStep  — ProviderMessage → NormalizedEmail
///   6. DeduplicateEmailsStep— skip already-persisted messages
///   7. PersistEmailsStep    — save to PostgreSQL
///   8. PublishDomainEventsStep — raise EmailReceived events via Outbox
///   9. RecordMetricsStep    — counters, histograms
///
/// Each step reads from and writes to <see cref="SyncContext"/>.
/// Steps MUST be idempotent — the pipeline may replay on partial failure.
/// </summary>
public interface ISyncPipelineStep
{
    string StepName { get; }

    Task ExecuteAsync(SyncContext context, CancellationToken cancellationToken = default);
}
