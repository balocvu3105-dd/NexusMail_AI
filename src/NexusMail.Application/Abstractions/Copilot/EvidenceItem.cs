namespace NexusMail.Application.Abstractions.Copilot;

public sealed record EvidenceItem
{
    public Guid EmailId { get; init; }
    public string Subject { get; init; } = string.Empty;
    public float RelevanceScore { get; init; }
}
