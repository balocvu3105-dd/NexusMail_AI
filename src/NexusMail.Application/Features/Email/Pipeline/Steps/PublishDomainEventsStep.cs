using Microsoft.Extensions.Logging;
using NexusMail.Application.Abstractions.Persistence;

namespace NexusMail.Application.Features.Email.Pipeline.Steps;

/// <summary>
/// Step 8 — Commit transaction for persisted emails.
///
/// Events are automatically published via the EF Core Outbox Interceptor.
/// This step commits the transaction, ensuring Emails, Outbox, and SyncState
/// are saved atomically.
///
/// Downstream consumers (via EventHandlers):
///   → AI Worker (summarize, classify, embed)
///   → Automation Worker (evaluate rules)
///   → Search Indexer (index for full-text + semantic search)
///
/// Writes: context.Result.EventsPublished
/// </summary>
public sealed class PublishDomainEventsStep : ISyncPipelineStep
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PublishDomainEventsStep> _logger;

    public string StepName => "PublishDomainEvents";

    public PublishDomainEventsStep(IUnitOfWork unitOfWork, ILogger<PublishDomainEventsStep> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ExecuteAsync(SyncContext context, CancellationToken cancellationToken = default)
    {
        if (context.PersistedEmailIds.Count == 0)
        {
            _logger.LogDebug("[{Step}] No persisted emails — skipping.", StepName);
            return;
        }

        var published = context.PersistedEmailIds.Count;

        // Advance cursor if this is the final page of a successful batch
        if (!context.HasMorePages && context.NextProviderCursor != null)
        {
            context.Account.State.MarkSuccess(published, context.NextProviderCursor);
        }

        // Save Emails, Outbox messages, and SyncState cursor in a single atomic transaction
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        context.Result.EventsPublished = published;

        _logger.LogDebug(
            "[{Step}] Committed {Count} emails (and their Outbox events) for account {AccountId}.",
            StepName, published, context.EmailAccountId);
    }
}
