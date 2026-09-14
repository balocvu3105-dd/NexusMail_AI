using System.Threading;
using System.Threading.Tasks;

namespace NexusMail.Application.Abstractions.Copilot;

public sealed record LLMSynthesisResult
{
    public string Answer { get; init; } = string.Empty;
    public GroundingState State { get; init; }
    public System.Collections.Generic.IReadOnlyList<ActionProposal> SuggestedActions { get; init; } = new System.Collections.Generic.List<ActionProposal>();
}

public record LanguageStreamChunk(string? Text, GroundingState? FinalState, System.Collections.Generic.IReadOnlyList<ActionProposal>? SuggestedActions);

public interface ICopilotLanguageService
{
    Task<LLMSynthesisResult> SynthesizeAnswerAsync(string query, string contextDocument, CancellationToken cancellationToken = default);
    IAsyncEnumerable<LanguageStreamChunk> SynthesizeStreamAsync(string query, string contextDocument, CancellationToken cancellationToken = default);
}
