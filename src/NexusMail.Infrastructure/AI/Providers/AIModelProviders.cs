using NexusMail.Application.Abstractions.AI;

namespace NexusMail.Infrastructure.AI.Providers;



/// <summary>
/// Anthropic Claude provider stub.
/// Sprint 5+: implement using Anthropic .NET SDK or direct HTTP.
/// Models: claude-3-5-sonnet-20241022 (completion), no native embedding (use OpenAI)
/// </summary>
public sealed class ClaudeModelProvider : IAIModelProvider
{
    public string ProviderName => "claude";
    public string ModelId => "claude-3-5-sonnet-20241022";
    public bool SupportsEmbedding => false;

    public Task<AICompletionResponse> CompleteAsync(AICompletionRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Claude provider — Sprint 5.");

    public IAsyncEnumerable<string> CompleteStreamAsync(AICompletionRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Claude provider stream — Sprint 5.");

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Claude does not support native embeddings. Use OpenAI or Gemini for embeddings.");
}

/// <summary>
/// Google Gemini provider stub.
/// Sprint 5+: implement using Google.AI.Generativelanguage NuGet.
/// Models: gemini-2.0-flash (completion), text-embedding-004 (embedding)
/// </summary>
public sealed class GeminiModelProvider : IAIModelProvider
{
    public string ProviderName => "gemini";
    public string ModelId => "gemini-2.0-flash";
    public bool SupportsEmbedding => true;

    public Task<AICompletionResponse> CompleteAsync(AICompletionRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Gemini provider — Sprint 5.");

    public IAsyncEnumerable<string> CompleteStreamAsync(AICompletionRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Gemini provider stream — Sprint 5.");

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Gemini embedding — Sprint 5.");
}

/// <summary>
/// Ollama local model provider stub.
/// Sprint 5+: implement using Ollama HTTP API (localhost:11434).
/// Benefits: privacy, no API costs, runs offline.
/// Models: llama3.2, mistral, phi3, nomic-embed-text (embedding)
/// </summary>
public sealed class OllamaModelProvider : IAIModelProvider
{
    public string ProviderName => "ollama";
    public string ModelId => "llama3.2";
    public bool SupportsEmbedding => true;

    public Task<AICompletionResponse> CompleteAsync(AICompletionRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Ollama provider — Sprint 5.");

    public IAsyncEnumerable<string> CompleteStreamAsync(AICompletionRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Ollama provider stream — Sprint 5.");

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Ollama embedding (nomic-embed-text) — Sprint 5.");
}
