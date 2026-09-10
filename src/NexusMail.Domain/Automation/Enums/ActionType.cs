namespace NexusMail.Domain.Automation.Enums;

/// <summary>
/// Actions that automation rules can execute.
/// Actions are independent from Email domain — they consume contract data only.
/// </summary>
public enum ActionType
{
    // ─── Email Actions ──────────────────────────────────────────────────────
    /// <summary>Apply a label/tag to the email.</summary>
    ApplyLabel = 0,

    /// <summary>Forward the email to one or more addresses.</summary>
    ForwardEmail = 1,

    /// <summary>Mark email as read.</summary>
    MarkAsRead = 2,

    /// <summary>Star / flag the email as important.</summary>
    StarEmail = 3,

    /// <summary>Move email to a specific folder.</summary>
    MoveToFolder = 4,

    /// <summary>Archive the email.</summary>
    Archive = 5,

    /// <summary>Generate and optionally send an auto-reply using AI.</summary>
    AutoReply = 6,

    // ─── Notification Actions ───────────────────────────────────────────────
    /// <summary>Send push notification via NexusMail notification system.</summary>
    SendNotification = 10,

    /// <summary>Send a message to a Discord webhook.</summary>
    NotifyDiscord = 11,

    /// <summary>Send a message to a Slack webhook.</summary>
    NotifySlack = 12,

    // ─── Integration Actions ────────────────────────────────────────────────
    /// <summary>Call an external HTTP webhook with email data.</summary>
    CallWebhook = 20,

    /// <summary>Create a task in an external task management system.</summary>
    CreateTask = 21,

    /// <summary>Save attachments to cloud storage (OneDrive, Google Drive, S3).</summary>
    SaveAttachments = 22,
}
