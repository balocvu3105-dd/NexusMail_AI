using Microsoft.Extensions.Logging;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Application.Features.Email.Pipeline.Models;

namespace NexusMail.Application.Features.Email.Pipeline.Steps;

/// <summary>
/// Step 6 — Deduplicate emails by checking if MessageId already exists in DB.
///
/// This step is CRITICAL for idempotency — the sync can run multiple times
/// and must not create duplicate emails.
///
/// Deduplication key: (EmailAccountId, MessageId) — globally unique per provider.
///
/// Writes: context.NewEmails, context.DuplicateCount
/// </summary>
public sealed class DeduplicateEmailsStep : ISyncPipelineStep
{
    private readonly IEmailPersistenceService _persistenceService;
    private readonly ILogger<DeduplicateEmailsStep> _logger;

    public string StepName => "DeduplicateEmails";

    public DeduplicateEmailsStep(
        IEmailPersistenceService persistenceService,
        ILogger<DeduplicateEmailsStep> logger)
    {
        _persistenceService = persistenceService;
        _logger = logger;
    }

    public async Task ExecuteAsync(SyncContext context, CancellationToken cancellationToken = default)
    {
        if (context.NormalizedEmails.Count == 0)
        {
            context.NewEmails = [];
            return;
        }

        // Bulk check — fetch existing message IDs in one DB query
        var incomingIds = context.NormalizedEmails.Select(e => e.MessageId).ToHashSet();
        var existingIds = await GetExistingMessageIdsAsync(
            context.EmailAccountId, incomingIds, cancellationToken);

        var newEmails = context.NormalizedEmails
            .Where(e => !existingIds.Contains(e.MessageId))
            .ToList();

        var duplicateCount = context.NormalizedEmails.Count - newEmails.Count;

        context.NewEmails = newEmails.AsReadOnly();
        context.DuplicateCount = duplicateCount;
        context.Result.Duplicates = duplicateCount;

        _logger.LogDebug(
            "[{Step}] New={New} Duplicates={Duplicates} for account {AccountId}",
            StepName, newEmails.Count, duplicateCount, context.EmailAccountId);
    }

    private Task<HashSet<string>> GetExistingMessageIdsAsync(
        Guid emailAccountId,
        HashSet<string> messageIds,
        CancellationToken cancellationToken)
    {
        return _persistenceService.GetExistingMessageIdsAsync(emailAccountId, messageIds, cancellationToken);
    }
}
