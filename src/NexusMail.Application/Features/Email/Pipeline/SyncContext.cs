using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Application.Features.Email.Pipeline.Models;
using NexusMail.Domain.Email.Entities;

namespace NexusMail.Application.Features.Email.Pipeline;

/// <summary>
/// Shared state passed through all pipeline steps.
/// Each step reads from and writes to this context.
///
/// IMPORTANT: SyncContext is not thread-safe — the pipeline runs steps sequentially.
/// </summary>
public sealed class SyncContext
{
    // ─── Input (set before pipeline starts) ────────────────────────────────
    public required Guid EmailAccountId { get; init; }
    public required string CorrelationId { get; init; }
    public required EmailAccount Account { get; init; }

    // ─── Populated by FetchEmailsStep ──────────────────────────────────────
    public string? AccessToken { get; set; }
    public IReadOnlyList<ProviderMessageSummary> FetchedMessageIds { get; set; } = [];
    public string? NextPageToken { get; set; }
    public bool HasMorePages => NextPageToken is not null;
    
    public string? CurrentProviderCursor { get; set; }
    public string? BaselineCursor { get; set; }
    public string? NextProviderCursor { get; set; }

    // ─── Populated by ValidateEmailsStep ───────────────────────────────────
    public IReadOnlyList<ProviderMessage> ValidMessages { get; set; } = [];
    public int InvalidCount { get; set; }

    // ─── Populated by NormalizeEmailsStep ──────────────────────────────────
    public IReadOnlyList<NormalizedEmail> NormalizedEmails { get; set; } = [];

    // ─── Populated by DeduplicateEmailsStep ────────────────────────────────
    public IReadOnlyList<NormalizedEmail> NewEmails { get; set; } = [];
    public int DuplicateCount { get; set; }

    // ─── Populated by PersistEmailsStep ────────────────────────────────────
    public IReadOnlyList<Guid> PersistedEmailIds { get; set; } = [];

    // ─── Pipeline Control ──────────────────────────────────────────────────
    /// <summary>Set to true to halt the pipeline (e.g. rate limit hit, auth failure).</summary>
    public bool ShouldAbort { get; set; }
    public string? AbortReason { get; set; }

    // ─── Result Tracking ──────────────────────────────────────────────────
    public SyncResult Result { get; } = new();

    public void Abort(string reason)
    {
        ShouldAbort = true;
        AbortReason = reason;
    }
}
