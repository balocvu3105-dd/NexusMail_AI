using Microsoft.Extensions.Logging;
using NexusMail.Application.Abstractions.Persistence;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Application.Features.Email.Pipeline.Models;

namespace NexusMail.Application.Features.Email.Pipeline.Steps;

/// <summary>
/// Step 7 — Persist new normalized emails to the database.
///
/// Writes: context.PersistedEmailIds
///
/// IMPORTANT: This step only persists — it does NOT publish events.
/// Events are raised in the next step (PublishDomainEventsStep) to ensure
/// events are only published for successfully persisted emails.
///
/// TODO Sprint 4: Replace with batch insert via IEmailRepository.
/// </summary>
public sealed class PersistEmailsStep : ISyncPipelineStep
{
    private readonly IEmailPersistenceService _persistenceService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PersistEmailsStep> _logger;

    public string StepName => "PersistEmails";

    public PersistEmailsStep(
        IEmailPersistenceService persistenceService,
        IUnitOfWork unitOfWork,
        ILogger<PersistEmailsStep> logger)
    {
        _persistenceService = persistenceService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ExecuteAsync(SyncContext context, CancellationToken cancellationToken = default)
    {
        if (context.NewEmails.Count == 0)
        {
            _logger.LogDebug("[{Step}] No new emails to persist.", StepName);
            return;
        }

        try
        {
            var emails = context.NewEmails.Select(ne => new NexusMail.Domain.Email.Entities.Email(
                context.EmailAccountId,
                context.Account.WorkspaceId,
                ne.MessageId,
                ne.SenderEmail,
                ne.Subject ?? string.Empty,
                ne.BodyText ?? string.Empty,
                ne.ReceivedAt
            )).ToList();

            await _persistenceService.SaveEmailsAsync(
                emails.Cast<object>().ToList().AsReadOnly(),
                cancellationToken);
            // _unitOfWork.SaveChangesAsync(cancellationToken) moved to PublishDomainEventsStep for atomicity
            var persistedIds = emails.Select(e => e.Id).ToList().AsReadOnly();

            context.PersistedEmailIds = persistedIds;
            context.Result.Persisted = persistedIds.Count;

            _logger.LogInformation(
                "[{Step}] Persisted {Count} emails for account {AccountId}.",
                StepName, persistedIds.Count, context.EmailAccountId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{Step}] Failed to persist emails for account {AccountId}.", StepName, context.EmailAccountId);
            context.Abort($"Persist failed: {ex.Message}");
        }
    }
}
