namespace NexusMail.Domain.Automation.Enums;

/// <summary>
/// What triggers an automation rule.
/// Automation subscribes to events — NOT to Email domain directly.
/// </summary>
public enum TriggerType
{
    /// <summary>A new email was received and passed through sync pipeline.</summary>
    EmailReceived = 0,

    /// <summary>An email was labeled/tagged (e.g., by AI).</summary>
    EmailLabeled = 1,

    /// <summary>Time-based trigger (cron expression).</summary>
    Scheduled = 2,

    /// <summary>Manual trigger — user clicks "Run" in UI.</summary>
    Manual = 3,

    /// <summary>Email was sent (from an account managed by NexusMail).</summary>
    EmailSent = 4,

    /// <summary>
    /// AI processing has completed for an email (all capabilities are in terminal state).
    /// Rules with this trigger type are evaluated in the second pass, after AIProcessingCompleted arrives.
    /// They have access to AI context (category, priorityscore, summary, tags, language) in the evaluation.
    /// </summary>
    AIProcessingCompleted = 5
}
