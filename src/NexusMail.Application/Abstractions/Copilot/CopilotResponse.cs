using System.Collections.Generic;

namespace NexusMail.Application.Abstractions.Copilot;

public sealed record CopilotResponse
{
    public string Answer { get; init; } = string.Empty;
    public GroundingState State { get; init; }
    
    // Application owns the provenance
    public IReadOnlyList<EvidenceItem> Evidence { get; init; } = new List<EvidenceItem>();
    public IReadOnlyList<ActionProposal> SuggestedActions { get; init; } = new List<ActionProposal>();
}
