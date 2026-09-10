using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Application.Features.Email.Pipeline;
using NexusMail.Domain.Email.Events;
using MediatR;

namespace NexusMail.Application.Features.Email.Services;

/// <summary>
/// Orchestrates email synchronization by building a SyncContext and running
/// the EmailSyncPipeline for a given account.
///
/// This service is intentionally thin — all business logic lives in pipeline steps.
/// </summary>
public sealed class EmailSynchronizationService : IEmailSynchronizationService
{
    private readonly IEmailAccountRepository _repository;
    private readonly ISynchronizationMetrics _metrics;
    private readonly EmailSyncPipeline _pipeline;
    private readonly IPublisher _publisher;
    private readonly ILogger<EmailSynchronizationService> _logger;

    public EmailSynchronizationService(
        IEmailAccountRepository repository,
        ISynchronizationMetrics metrics,
        EmailSyncPipeline pipeline,
        IPublisher publisher,
        ILogger<EmailSynchronizationService> logger)
    {
        _repository = repository;
        _metrics = metrics;
        _pipeline = pipeline;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task SyncEmailAccountAsync(Guid emailAccountId, string correlationId, CancellationToken cancellationToken = default)
    {
        var account = await _repository.GetByIdAsync(emailAccountId, cancellationToken);
        if (account is null)
        {
            _logger.LogWarning("Account {EmailAccountId} not found. Skipping sync.", emailAccountId);
            return;
        }

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["WorkspaceId"] = account.WorkspaceId
        });
        
        System.Diagnostics.Activity.Current?.AddBaggage("WorkspaceId", account.WorkspaceId.ToString());
        System.Diagnostics.Activity.Current?.AddBaggage("CorrelationId", correlationId);

        _logger.LogInformation("Starting sync for account {EmailAccountId}", emailAccountId);

        _metrics.IncrementRunningWorkers();

        try
        {
            var context = new SyncContext
            {
                EmailAccountId = emailAccountId,
                CorrelationId = correlationId,
                Account = account,
                CurrentProviderCursor = account.State.ProviderCursor
            };

            var result = await _pipeline.RunAsync(context, cancellationToken);

            if (result.WasAborted)
            {
                _logger.LogWarning("Sync aborted for account {EmailAccountId}. Reason: {Reason}", emailAccountId, result.AbortReason);
                _metrics.IncrementFailedSyncs();
                await _publisher.Publish(new EmailSyncFailedEvent(emailAccountId, result.AbortReason ?? "Unknown"), cancellationToken);
            }
            else
            {
                account.State.MarkSuccess(result.Persisted, context.NextProviderCursor);
                await _repository.UpdateAsync(account, cancellationToken);
                await _publisher.Publish(new EmailSyncCompletedEvent(emailAccountId, result.Persisted), cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error during sync for account {EmailAccountId}", emailAccountId);
            _metrics.IncrementFailedSyncs();
            _metrics.IncrementRetryCount();
            await _publisher.Publish(new EmailSyncFailedEvent(emailAccountId, ex.Message), cancellationToken);
        }
        finally
        {
            _metrics.DecrementRunningWorkers();
        }
    }
}
