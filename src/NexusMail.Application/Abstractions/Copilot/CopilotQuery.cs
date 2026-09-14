namespace NexusMail.Application.Abstractions.Copilot;

public sealed record CopilotQuery
{
    public Guid WorkspaceId { get; init; }
    public string QueryText { get; init; } = string.Empty;
}
