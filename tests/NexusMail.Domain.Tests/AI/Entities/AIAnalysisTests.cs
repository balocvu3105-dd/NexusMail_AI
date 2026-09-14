using System;
using NexusMail.Domain.AI.Entities;
using NexusMail.Domain.AI.Enums;
using Xunit;
using System.Reflection;

namespace NexusMail.Domain.Tests.AI.Entities;

public class AIAnalysisTests
{
    private void SetPrivatePropertyValue<T>(object obj, string propName, T val)
    {
        var property = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property != null)
        {
            property.SetValue(obj, val);
        }
    }

    [Fact]
    public void StartProcessing_WhenPending_ShouldReturnTrueAndSetStateToProcessing()
    {
        var analysis = AIAnalysis.Create(Guid.NewGuid());
        var attemptId = Guid.NewGuid();

        var result = analysis.StartProcessing(attemptId);

        Assert.True(result);
        Assert.Equal(AIProcessingStatus.Processing, analysis.ProcessingState);
        Assert.Equal(attemptId, analysis.ProcessingAttemptId);
        Assert.NotNull(analysis.ProcessingStartedAt);
    }

    [Fact]
    public void StartProcessing_WhenProcessingLessThan5Minutes_ShouldReturnFalse()
    {
        var analysis = AIAnalysis.Create(Guid.NewGuid());
        var initialAttemptId = Guid.NewGuid();
        analysis.StartProcessing(initialAttemptId);

        // Set ProcessingStartedAt to 2 minutes ago
        SetPrivatePropertyValue(analysis, "ProcessingStartedAt", DateTimeOffset.UtcNow.AddMinutes(-2));

        var newAttemptId = Guid.NewGuid();
        var result = analysis.StartProcessing(newAttemptId);

        Assert.False(result); // Cannot claim
        Assert.Equal(initialAttemptId, analysis.ProcessingAttemptId); // Kept the old one
    }

    [Fact]
    public void StartProcessing_WhenProcessingMoreThan5Minutes_ShouldClaimAndReturnTrue()
    {
        var analysis = AIAnalysis.Create(Guid.NewGuid());
        var initialAttemptId = Guid.NewGuid();
        analysis.StartProcessing(initialAttemptId);

        // Set ProcessingStartedAt to 6 minutes ago
        SetPrivatePropertyValue(analysis, "ProcessingStartedAt", DateTimeOffset.UtcNow.AddMinutes(-6));

        var newAttemptId = Guid.NewGuid();
        var result = analysis.StartProcessing(newAttemptId);

        Assert.True(result); // Claimed successfully
        Assert.Equal(newAttemptId, analysis.ProcessingAttemptId);
    }

    [Fact]
    public void StartProcessing_WhenCompleted_ShouldReturnFalse()
    {
        var analysis = AIAnalysis.Create(Guid.NewGuid());
        var attemptId = Guid.NewGuid();
        analysis.StartProcessing(attemptId);
        analysis.CompleteProcessing(attemptId, "Summary", 100, "Reason", "Work", 0.9, null, false);

        var newAttemptId = Guid.NewGuid();
        var result = analysis.StartProcessing(newAttemptId);

        Assert.False(result); // Skip
    }

    [Fact]
    public void StartProcessing_WhenFailed_ShouldReturnTrueAndRestart()
    {
        var analysis = AIAnalysis.Create(Guid.NewGuid());
        var attemptId = Guid.NewGuid();
        analysis.StartProcessing(attemptId);
        analysis.FailProcessing(attemptId, "Error");

        var newAttemptId = Guid.NewGuid();
        var result = analysis.StartProcessing(newAttemptId);

        Assert.True(result);
        Assert.Equal(AIProcessingStatus.Processing, analysis.ProcessingState);
        Assert.Equal(newAttemptId, analysis.ProcessingAttemptId);
    }
}
