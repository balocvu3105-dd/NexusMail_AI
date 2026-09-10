using System;

namespace NexusMail.Application.Abstractions.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    
    public string Type { get; set; } = string.Empty;
    
    public string Content { get; set; } = string.Empty;
    
    public DateTimeOffset OccurredOnUtc { get; set; }
    
    public DateTimeOffset? ProcessedOnUtc { get; set; }
    
    public string? Error { get; set; }
    
    public int Attempts { get; set; }
    
    public string? LockId { get; set; }
    
    public DateTimeOffset? LockedUntilUtc { get; set; }
    
    public string? CorrelationId { get; set; }
    
    public string? Headers { get; set; }

    public DateTimeOffset? DeadLetteredAt { get; set; }
    
    public FailureReason? FailureReason { get; set; }
}
