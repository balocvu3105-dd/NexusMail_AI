namespace NexusMail.Application.Features.Email.Pipeline.Models;

/// <summary>
/// Final result of a complete sync pipeline run for one email account.
/// Aggregated across all steps.
/// </summary>
public sealed class SyncResult
{
    public int Fetched { get; set; }
    public int Valid { get; set; }
    public int Invalid { get; set; }
    public int Normalized { get; set; }
    public int Duplicates { get; set; }
    public int Persisted { get; set; }
    public int EventsPublished { get; set; }
    public int RetryCount { get; set; }
    public int RateLimitHits { get; set; }
    public TimeSpan Duration { get; set; }
    public bool WasAborted { get; set; }
    public string? AbortReason { get; set; }

    public int Failed => Fetched - Persisted - Duplicates;

    public override string ToString()
        => $"Fetched={Fetched} Valid={Valid} Duplicates={Duplicates} Persisted={Persisted} " +
           $"Retries={RetryCount} RateLimitHits={RateLimitHits} Duration={Duration.TotalSeconds:F2}s";
}
