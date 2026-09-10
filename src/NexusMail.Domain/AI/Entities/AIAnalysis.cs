using System;
using System.Collections.Generic;
using NexusMail.Shared.Domain;
using NexusMail.Domain.AI.Events;
using NexusMail.Domain.AI.Enums;

namespace NexusMail.Domain.AI.Entities;

public class AIAnalysis : AggregateRoot
{
    public Guid EmailId { get; private set; }

    // Summary Capability
    public AIProcessingStatus SummaryStatus { get; private set; }
    public Guid? SummaryAttemptId { get; private set; }
    public string Summary { get; private set; } = string.Empty;
    public string? SummaryError { get; private set; }

    // Priority Capability
    public AIProcessingStatus PriorityStatus { get; private set; }
    public Guid? PriorityAttemptId { get; private set; }
    public int PriorityScore { get; private set; }
    public string Priority { get; private set; } = string.Empty;
    public string? PriorityError { get; private set; }

    // Classification Capability
    public AIProcessingStatus ClassificationStatus { get; private set; }
    public Guid? ClassificationAttemptId { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string Language { get; private set; } = string.Empty;
    public double Confidence { get; private set; }
    public List<string> Tags { get; private set; } = new();
    public string? ClassificationError { get; private set; }

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
            SummaryStatus = AIProcessingStatus.Pending,
            PriorityStatus = AIProcessingStatus.Pending,
            ClassificationStatus = AIProcessingStatus.Pending,
            EmbeddingStatus = AIProcessingStatus.Pending
        };
    }

    // --- SUMMARY ---
    public bool StartSummary(Guid attemptId)
    {
        if (SummaryStatus == AIProcessingStatus.Succeeded || SummaryStatus == AIProcessingStatus.Processing)
            return false;

        SummaryStatus = AIProcessingStatus.Processing;
        SummaryAttemptId = attemptId;
        SummaryError = null;
        return true;
    }

    public void CompleteSummary(Guid attemptId, string summary)
    {
        if (SummaryAttemptId != attemptId) return;
        SummaryStatus = AIProcessingStatus.Succeeded;
        Summary = summary;
        CheckCompletion();
    }

    public void FailSummary(Guid attemptId, string error)
    {
        if (SummaryAttemptId != attemptId) return;
        SummaryStatus = AIProcessingStatus.Failed;
        SummaryError = error;
        CheckCompletion();
    }

    public void ResetSummaryToPending(Guid attemptId)
    {
        if (SummaryAttemptId != attemptId) return;
        SummaryStatus = AIProcessingStatus.Pending;
        SummaryAttemptId = null;
    }

    // --- PRIORITY ---
    public bool StartPriority(Guid attemptId)
    {
        if (PriorityStatus == AIProcessingStatus.Succeeded || PriorityStatus == AIProcessingStatus.Processing)
            return false;

        PriorityStatus = AIProcessingStatus.Processing;
        PriorityAttemptId = attemptId;
        PriorityError = null;
        return true;
    }

    public void CompletePriority(Guid attemptId, int score, string priority)
    {
        if (PriorityAttemptId != attemptId) return;
        PriorityStatus = AIProcessingStatus.Succeeded;
        PriorityScore = score;
        Priority = priority;
        CheckCompletion();
    }

    public void FailPriority(Guid attemptId, string error)
    {
        if (PriorityAttemptId != attemptId) return;
        PriorityStatus = AIProcessingStatus.Failed;
        PriorityError = error;
        CheckCompletion();
    }

    public void ResetPriorityToPending(Guid attemptId)
    {
        if (PriorityAttemptId != attemptId) return;
        PriorityStatus = AIProcessingStatus.Pending;
        PriorityAttemptId = null;
    }

    // --- CLASSIFICATION ---
    public bool StartClassification(Guid attemptId)
    {
        if (ClassificationStatus == AIProcessingStatus.Succeeded || ClassificationStatus == AIProcessingStatus.Processing)
            return false;

        ClassificationStatus = AIProcessingStatus.Processing;
        ClassificationAttemptId = attemptId;
        ClassificationError = null;
        return true;
    }

    public void CompleteClassification(Guid attemptId, string category, double confidence, List<string>? tags = null)
    {
        if (ClassificationAttemptId != attemptId) return;
        ClassificationStatus = AIProcessingStatus.Succeeded;
        Category = category;
        Confidence = confidence;
        if (tags != null) Tags = tags;

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

    public void FailClassification(Guid attemptId, string error)
    {
        if (ClassificationAttemptId != attemptId) return;
        ClassificationStatus = AIProcessingStatus.Failed;
        ClassificationError = error;
        CheckCompletion();
    }

    public void ResetClassificationToPending(Guid attemptId)
    {
        if (ClassificationAttemptId != attemptId) return;
        ClassificationStatus = AIProcessingStatus.Pending;
        ClassificationAttemptId = null;
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

    public void CompleteEmbedding(Guid attemptId)
    {
        if (EmbeddingAttemptId != attemptId) return;
        EmbeddingStatus = AIProcessingStatus.Succeeded;
        CheckCompletion();
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

    private void CheckCompletion()
    {
        if (IsCompletedEventPublished) return;

        bool isTerminal(AIProcessingStatus status) => status == AIProcessingStatus.Succeeded || status == AIProcessingStatus.Failed;

        if (isTerminal(SummaryStatus) &&
            isTerminal(PriorityStatus) &&
            isTerminal(ClassificationStatus) &&
            isTerminal(EmbeddingStatus))
        {
            IsCompletedEventPublished = true;
            
            AddDomainEvent(new EmailAIProcessingCompleted
            {
                EmailId = EmailId,
                SummarySucceeded = SummaryStatus == AIProcessingStatus.Succeeded,
                PrioritySucceeded = PriorityStatus == AIProcessingStatus.Succeeded,
                ClassificationSucceeded = ClassificationStatus == AIProcessingStatus.Succeeded,
                EmbeddingSucceeded = EmbeddingStatus == AIProcessingStatus.Succeeded,
                Summary = SummaryStatus == AIProcessingStatus.Succeeded ? Summary : null,
                PriorityScore = PriorityStatus == AIProcessingStatus.Succeeded ? PriorityScore : null,
                Category = ClassificationStatus == AIProcessingStatus.Succeeded ? Category : null,
                Language = ClassificationStatus == AIProcessingStatus.Succeeded ? Language : null,
                Tags = ClassificationStatus == AIProcessingStatus.Succeeded ? Tags : null
            });
        }
    }
}
