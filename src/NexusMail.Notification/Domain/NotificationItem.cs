using System;
using NexusMail.Shared.Domain;

namespace NexusMail.Notification.Domain;

public sealed class NotificationItem : AggregateRoot
{
    public Guid WorkspaceId { get; private set; }
    public NotificationType Type { get; private set; }
    
    // Idempotency guardrail: Links to the IntegrationEvent's ID to prevent duplicates
    public string SourceEventId { get; private set; } = string.Empty;
    
    // Core payload fields
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string? ActionUrl { get; private set; } 
    
    // Flexible data bag for specific event contexts (e.g. EmailId, RuleId)
    // Structured schema handled by Backend
    public string PayloadJson { get; private set; } = string.Empty;
    
    public bool IsRead { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private NotificationItem() { }

    public static NotificationItem Create(
        Guid workspaceId,
        NotificationType type,
        string sourceEventId,
        string title,
        string message,
        string payloadJson,
        string? actionUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceEventId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return new NotificationItem
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Type = type,
            SourceEventId = sourceEventId,
            Title = title,
            Message = message,
            ActionUrl = actionUrl,
            PayloadJson = payloadJson,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
