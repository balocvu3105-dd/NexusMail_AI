using System;

namespace NexusMail.Contracts.Automation;

/// <summary>
/// Triggered when the AutoReply action rule fires.
/// The AI Worker should consume this, generate a draft, and save it for human approval.
/// </summary>
public sealed record DraftGenerationRequestedEvent
{
    public Guid WorkspaceId { get; init; }
    public Guid EmailId { get; init; }
    public string PromptTemplate { get; init; } = string.Empty;
}
