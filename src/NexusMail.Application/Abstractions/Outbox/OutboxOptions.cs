using System;

namespace NexusMail.Application.Abstractions.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public int MaxAttempts { get; set; } = 5;
    
    public TimeSpan LeaseTimeout { get; set; } = TimeSpan.FromMinutes(2);
    
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(10);
    
    public int BatchSize { get; set; } = 20;
}
