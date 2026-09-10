namespace NexusMail.Application.Abstractions.AI;

/// <summary>
/// Completion response from an AI model provider.
/// Normalized across providers — caller does not need to know the underlying API format.
/// </summary>
public sealed class AICompletionResponse
{
    public required string Content { get; init; }
    public required string ProviderName { get; init; }
    public required string ModelId { get; init; }
    public required string PromptVersion { get; init; }
    public AITokenUsage Usage { get; init; } = AITokenUsage.Empty;
    public TimeSpan Latency { get; init; } = TimeSpan.Zero;
    public decimal EstimatedCost { get; init; } = 0m;
    public bool IsSuccess { get; init; } = true;
    public string? ErrorMessage { get; init; }

    public static AICompletionResponse Failure(string providerName, string modelId, string promptVersion, string error)
        => new()
        {
            Content = string.Empty,
            ProviderName = providerName,
            ModelId = modelId,
            PromptVersion = promptVersion,
            IsSuccess = false,
            ErrorMessage = error
        };
}

/// <summary>Token usage for billing and quota tracking.</summary>
public sealed record AITokenUsage(int PromptTokens, int CompletionTokens)
{
    public int TotalTokens => PromptTokens + CompletionTokens;
    public static readonly AITokenUsage Empty = new(0, 0);
}
