using System;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Application.Abstractions.AI;

namespace NexusMail.IntegrationTests;

public class FakeAIModelProvider : IAIModelProvider
{
    public string ProviderName => "fake_ai";
    public string ModelId => "fake-model-id";
    public bool SupportsEmbedding => true;

    public Func<AICompletionRequest, AICompletionResponse>? CompletionBehavior { get; set; }

    public Task<AICompletionResponse> CompleteAsync(AICompletionRequest request, CancellationToken cancellationToken = default)
    {
        if (CompletionBehavior != null)
        {
            return Task.FromResult(CompletionBehavior(request));
        }

        // Default successful behavior matching the JSON schema
        var defaultJson = """
        {
          "classification": {
            "category": "Personal",
            "confidence": 0.95,
            "tags": ["fake-tag"]
          },
          "priority": {
            "score": 50,
            "reason": "Fake reasoning"
          },
          "summary": "Fake summary",
          "needsAttention": false
        }
        """;

        return Task.FromResult(new AICompletionResponse
        {
            Content = defaultJson,
            ProviderName = ProviderName,
            ModelId = ModelId,
            PromptVersion = request.PromptVersion,
            IsSuccess = true
        });
    }

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new float[] { 0.1f, 0.2f, 0.3f });
    }
}
