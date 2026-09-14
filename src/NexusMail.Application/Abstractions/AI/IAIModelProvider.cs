namespace NexusMail.Application.Abstractions.AI;

/// <summary>
/// Low-level abstraction over a specific AI model provider (OpenAI, Claude, Gemini, Ollama...).
///
/// APPLICATION CODE MUST NOT use this interface directly.
/// Use <see cref="IAIService"/> instead — it provides high-level operations
/// and selects the appropriate model internally.
///
/// DESIGN: Custom abstraction (NOT Microsoft.Extensions.AI) to avoid lock-in
/// to Microsoft's preview API which changes frequently.
/// </summary>
public interface IAIModelProvider
{
    /// <summary>Unique name of this provider (e.g., "openai", "claude", "gemini", "ollama").</summary>
    string ProviderName { get; }

    /// <summary>The model identifier used for completions (e.g., "gpt-4o", "claude-3-5-sonnet").</summary>
    string ModelId { get; }

    /// <summary>Whether this provider supports generating embeddings.</summary>
    bool SupportsEmbedding { get; }

    /// <summary>
    /// Send a completion request (chat/text generation).
    /// </summary>
    Task<AICompletionResponse> CompleteAsync(AICompletionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a streaming completion request.
    /// </summary>
    IAsyncEnumerable<string> CompleteStreamAsync(AICompletionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a vector embedding for the given text.
    /// </summary>
    /// <exception cref="NotSupportedException">When <see cref="SupportsEmbedding"/> is false.</exception>
    Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default);
}
