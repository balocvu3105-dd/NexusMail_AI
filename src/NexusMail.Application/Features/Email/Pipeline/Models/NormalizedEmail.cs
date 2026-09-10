namespace NexusMail.Application.Features.Email.Pipeline.Models;

/// <summary>
/// Provider-agnostic representation of an email after normalization.
/// This is the canonical model the rest of the system works with —
/// NOT the raw ProviderMessage which is provider-specific.
/// </summary>
public sealed record NormalizedEmail
{
    public required string MessageId { get; init; }    // Provider's unique ID
    public required string ThreadId { get; init; }
    public required string Subject { get; init; }
    public required string Sender { get; init; }       // "display name <email@domain.com>"
    public required string SenderEmail { get; init; }  // Parsed email address
    public required string SenderName { get; init; }   // Parsed display name
    public IReadOnlyList<string> To { get; init; } = [];
    public IReadOnlyList<string> Cc { get; init; } = [];
    public string Preview { get; init; } = string.Empty;
    public string? BodyText { get; init; }
    public string? BodyHtml { get; init; }
    public bool HasAttachments { get; init; }
    public IReadOnlyList<string> Labels { get; init; } = [];
    public DateTimeOffset ReceivedAt { get; init; }
    public bool IsRead { get; init; }
    public bool IsStarred { get; init; }
}
