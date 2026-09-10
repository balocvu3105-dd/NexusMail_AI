namespace NexusMail.Application.Abstractions.AI;

/// <summary>
/// Completion request sent to an AI model provider.
/// Provider-agnostic — works with OpenAI, Claude, Gemini, Ollama, etc.
/// </summary>
public sealed class AICompletionRequest
{
    public required IReadOnlyList<AIMessage> Messages { get; init; }
    public double Temperature { get; init; } = 0.3;
    public int MaxTokens { get; init; } = 512;
    public string? SystemPrompt { get; init; }
    public AIResponseFormat ResponseFormat { get; init; } = AIResponseFormat.Text;

    /// <summary>
    /// If ResponseFormat == Json, this schema enforces the exact JSON structure returned.
    /// (e.g., using OpenAI Structured Outputs).
    /// </summary>
    public string? JsonSchema { get; init; }

    /// <summary>
    /// Version of the prompt used (e.g., "Summary_v1").
    /// </summary>
    public string PromptVersion { get; init; } = "v1";
}

/// <summary>
/// A single message in the conversation (role + content).
/// </summary>
public sealed record AIMessage(AIRole Role, string Content);

public enum AIRole { System, User, Assistant }
public enum AIResponseFormat { Text, Json }
