using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NexusMail.Application.Features.Notification.DTOs;

public sealed record NotificationDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? ActionUrl { get; init; }
    
    // Deserialize the raw JSON string into a structured object for the frontend
    public JsonDocument? Payload { get; init; }
    
    public bool IsRead { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
