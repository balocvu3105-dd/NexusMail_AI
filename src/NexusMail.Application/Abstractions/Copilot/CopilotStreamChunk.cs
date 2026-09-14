using System.Collections.Generic;

namespace NexusMail.Application.Abstractions.Copilot;

public sealed record CopilotStreamChunk
{
    public string? ChunkText { get; init; }
    public GroundingState? State { get; init; }
    public IReadOnlyList<EvidenceItem>? Evidence { get; init; }
    public IReadOnlyList<ActionProposal>? SuggestedActions { get; init; }
    public bool IsDone { get; init; }
}
