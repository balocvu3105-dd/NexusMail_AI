using System;
using System.Collections.Generic;
using NexusMail.Shared.Domain;
using NexusMail.Domain.AI.Events;
using NexusMail.Domain.AI.Enums;

namespace NexusMail.Domain.AI.Entities;

public class AIAnalysis : AggregateRoot
{
    public Guid EmailId { get; private set; }

    // Consolidated Processing Capability (Classification, Priority, Summary, NeedsAttention)
    public AIProcessingStatus ProcessingState { get; private set; }
    public Guid? ProcessingAttemptId { get; private set; }
    public DateTimeOffset? ProcessingStartedAt { get; private set; }
    public string? ProcessingError { get; private set; }

    public string Summary { get; private set; } = string.Empty;
    public int PriorityScore { get; private set; }
    public string Priority { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Language { get; private set; } = string.Empty;
    public double Confidence { get; private set; }
    public List<string> Tags { get; private set; } = new();
    public bool NeedsAttention { get; private set; }

    // Embedding Capability
    public AIProcessingStatus EmbeddingStatus { get; private set; }
    public Guid? EmbeddingAttemptId { get; private set; }
    public string? EmbeddingError { get; private set; }
    public bool IsCompletedEventPublished { get; private set; }

    private AIAnalysis() { }

    public static AIAnalysis Create(Guid emailId)
    {
        return new AIAnalysis
        {
            Id = Guid.NewGuid(),
            EmailId = emailId,
            ProcessingState = AIProcessingStatus.Pending,
            EmbeddingStatus = AIProcessingStatus.Pending
        };
    }

    // --- UNIFIED PROCESSING ---
    public bool StartProcessing(Guid attemptId)
    {
        if (ProcessingState == AIProcessingStatus.Succeeded)
            return false;

        // Stale recovery: if Processing but started > 5 minutes ago, we can claim it
        if (ProcessingState == AIProcessingStatus.Processing && ProcessingStartedAt.HasValue)
        {
            if (ProcessingStartedAt.Value > DateTimeOffset.UtcNow.AddMinutes(-5))
            {
                return false; // Still processing and not stale
            }
        }

        ProcessingState = AIProcessingStatus.Processing;
        ProcessingAttemptId = attemptId;
        ProcessingStartedAt = DateTimeOffset.UtcNow;
        ProcessingError = null;
        return true;
    }

    public void CompleteProcessing(Guid attemptId, string summary, int priorityScore, string priority, string category, double confidence, List<string>? tags, bool needsAttention)
    {
        if (ProcessingAttemptId != attemptId) return;
        ProcessingState = AIProcessingStatus.Succeeded;
        Summary = summary;
        PriorityScore = priorityScore;
        Priority = priority;
        Category = category;
        Confidence = confidence;
        if (tags != null) Tags = tags;
        NeedsAttention = needsAttention;

        AddDomainEvent(new EmailAnalyzed
        {
            EmailId = EmailId,
            Language = Language,
            Category = Category,
            Confidence = Confidence,
            Tags = Tags
        });
        
        CheckCompletion();
    }

    public void FailProcessing(Guid attemptId, string error)
    {
        if (ProcessingAttemptId != attemptId) return;
        ProcessingState = AIProcessingStatus.Failed;
        ProcessingError = error;
        CheckCompletion();
    }

    public void ResetProcessingToPending(Guid attemptId)
    {
        if (ProcessingAttemptId != attemptId) return;
        ProcessingState = AIProcessingStatus.Pending;
        ProcessingAttemptId = null;
        ProcessingStartedAt = null;
    }

    // --- EMBEDDING ---
    public bool StartEmbedding(Guid attemptId)
    {
        if (EmbeddingStatus == AIProcessingStatus.Succeeded || EmbeddingStatus == AIProcessingStatus.Processing)
            return false;

        EmbeddingStatus = AIProcessingStatus.Processing;
        EmbeddingAttemptId = attemptId;
        EmbeddingError = null;
        return true;
    }

    public void CompleteEmbedding(Guid attemptId, float[]? embeddingVector = null)
    {
        if (EmbeddingAttemptId != attemptId) return;
        EmbeddingStatus = AIProcessingStatus.Succeeded;
        CheckCompletion(embeddingVector);
    }

    public void FailEmbedding(Guid attemptId, string error)
    {
        if (EmbeddingAttemptId != attemptId) return;
        EmbeddingStatus = AIProcessingStatus.Failed;
        EmbeddingError = error;
        CheckCompletion();
    }

    public void ResetEmbeddingToPending(Guid attemptId)
    {
        if (EmbeddingAttemptId != attemptId) return;
        EmbeddingStatus = AIProcessingStatus.Pending;
        EmbeddingAttemptId = null;
    }

    private void CheckCompletion(float[]? embeddingVector = null)
    {
        if (IsCompletedEventPublished) return;

        bool isTerminal(AIProcessingStatus status) => status == AIProcessingStatus.Succeeded || status == AIProcessingStatus.Failed;

        if (isTerminal(ProcessingState) &&
            isTerminal(EmbeddingStatus))
        {
            IsCompletedEventPublished = true;
            
            AddDomainEvent(new EmailAIProcessingCompleted
            {
                EmailId = EmailId,
                SummarySucceeded = ProcessingState == AIProcessingStatus.Succeeded,
                PrioritySucceeded = ProcessingState == AIProcessingStatus.Succeeded,
                ClassificationSucceeded = ProcessingState == AIProcessingStatus.Succeeded,
                EmbeddingSucceeded = EmbeddingStatus == AIProcessingStatus.Succeeded,
                Summary = ProcessingState == AIProcessingStatus.Succeeded ? Summary : null,
                PriorityScore = ProcessingState == AIProcessingStatus.Succeeded ? PriorityScore : null,
                Category = ProcessingState == AIProcessingStatus.Succeeded ? Category : null,
                Language = ProcessingState == AIProcessingStatus.Succeeded ? Language : null,
                Tags = ProcessingState == AIProcessingStatus.Succeeded ? Tags : null,
                EmbeddingVector = embeddingVector
            });
        }
    }
}
