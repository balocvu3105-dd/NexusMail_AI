using System;
using NexusMail.Domain.Email.Enums;
using NexusMail.Shared.Domain;

namespace NexusMail.Domain.Email.Entities;

public sealed class EmailSynchronizationState : EntityBase<Guid>
{
    private EmailSynchronizationState(Guid id, Guid emailAccountId) : base(id)
    {
        EmailAccountId = emailAccountId;
        Status = EmailSyncStatus.Pending;
    }

    private EmailSynchronizationState() { } // For EF Core

    public Guid EmailAccountId { get; private set; }
    public EmailSyncStatus Status { get; private set; }
    
    public EmailSyncStage CurrentStage { get; private set; }
    
    public int RetryCount { get; private set; }
    public DateTime? NextRetryAt { get; private set; }
    
    public DateTime? LastAttemptAt { get; private set; }
    public DateTime? LastSuccessAt { get; private set; }
    public string? LastError { get; private set; }
    
    public string? CorrelationId { get; private set; }
    
    public string? ProviderCursor { get; private set; }
    
    // Concurrency Token
    public uint Version { get; private set; }

    public static EmailSynchronizationState Create(Guid emailAccountId)
    {
        return new EmailSynchronizationState(Guid.NewGuid(), emailAccountId);
    }

    public void MarkAttemptStarted(string correlationId)
    {
        Status = EmailSyncStatus.Syncing;
        CurrentStage = EmailSyncStage.Pending;
        LastAttemptAt = DateTime.UtcNow;
        CorrelationId = correlationId;
    }
    
    public void UpdateStage(EmailSyncStage stage)
    {
        CurrentStage = stage;
    }

    public void MarkSuccess(int emailsSyncedCount, string? newProviderCursor = null)
    {
        Status = EmailSyncStatus.Idle;
        CurrentStage = EmailSyncStage.Completed;
        LastSuccessAt = DateTime.UtcNow;
        RetryCount = 0;
        NextRetryAt = null;
        LastError = null;
        
        if (newProviderCursor != null)
        {
            ProviderCursor = newProviderCursor;
        }
    }

    public void MarkFailed(string error, int maxRetries, TimeSpan nextRetryDelay)
    {
        LastError = error;
        CurrentStage = EmailSyncStage.Failed;
        RetryCount++;
        
        if (RetryCount >= maxRetries)
        {
            Status = EmailSyncStatus.Failed;
            NextRetryAt = null;
        }
        else
        {
            Status = EmailSyncStatus.Retrying;
            NextRetryAt = DateTime.UtcNow.Add(nextRetryDelay);
        }
    }
}
