namespace NexusMail.Application.Abstractions.Copilot;

public enum GroundingState
{
    /// <summary>
    /// LLM determined that the provided context contains sufficient evidence to answer the query.
    /// </summary>
    Grounded,

    /// <summary>
    /// LLM determined that the provided context does NOT contain sufficient evidence to answer the query.
    /// Unknown is not False.
    /// </summary>
    InsufficientEvidence
}
