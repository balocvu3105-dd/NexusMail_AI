using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Abstractions.AI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using Polly;
using Polly.Retry;

namespace NexusMail.Infrastructure.AI.Providers;

/// <summary>
/// Implementation of IAIModelProvider using the official .NET OpenAI SDK.
/// Supports chat completions, structured outputs, and embeddings.
/// </summary>
public sealed class OpenAIModelProvider : IAIModelProvider
{
    private readonly ChatClient _chatClient;
    private readonly EmbeddingClient _embeddingClient;
    private readonly ILogger<OpenAIModelProvider> _logger;
    private readonly ResiliencePipeline _retryPipeline;
    private readonly string _modelId;
    private readonly string _embeddingModelId;

    public OpenAIModelProvider(IConfiguration configuration, ILogger<OpenAIModelProvider> logger)
    {
        _logger = logger;
        
        var apiKey = configuration["AI:OpenAI:ApiKey"] 
            ?? throw new InvalidOperationException("Missing AI:OpenAI:ApiKey configuration.");
            
        _modelId = configuration["AI:OpenAI:ModelId"] ?? "gpt-4o-mini";
        _embeddingModelId = configuration["AI:OpenAI:EmbeddingModelId"] ?? "text-embedding-3-small";

        _chatClient = new ChatClient(_modelId, apiKey);
        _embeddingClient = new EmbeddingClient(_embeddingModelId, apiKey);
        
        // Define Polly retry pipeline (Exponential Backoff for 429, 500, 502, 503)
        _retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(ex => 
                    ex.Message.Contains("429") || 
                    ex.Message.Contains("500") || 
                    ex.Message.Contains("502") || 
                    ex.Message.Contains("503") ||
                    ex.Message.Contains("Timeout")),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args => 
                {
                    _logger.LogWarning("Retrying OpenAI call due to {Exception}. Attempt {Attempt}", args.Outcome.Exception?.Message, args.AttemptNumber);
                    return default;
                }
            })
            .Build();
    }

    public string ProviderName => "openai";
    public string ModelId => _modelId;
    public bool SupportsEmbedding => true;

    public async Task<AICompletionResponse> CompleteAsync(AICompletionRequest request, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>();

        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            messages.Add(new SystemChatMessage(request.SystemPrompt));
        }

        foreach (var msg in request.Messages)
        {
            ChatMessage chatMsg = msg.Role switch
            {
                AIRole.System => new SystemChatMessage(msg.Content),
                AIRole.Assistant => new AssistantChatMessage(msg.Content),
                _ => new UserChatMessage(msg.Content)
            };
            messages.Add(chatMsg);
        }

        var options = new ChatCompletionOptions
        {
            Temperature = (float)request.Temperature,
            MaxOutputTokenCount = request.MaxTokens
        };

        if (request.ResponseFormat == AIResponseFormat.Json)
        {
            if (!string.IsNullOrWhiteSpace(request.JsonSchema))
            {
                options.ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "structured_output",
                    jsonSchema: BinaryData.FromString(request.JsonSchema),
                    jsonSchemaIsStrict: true);
            }
            else
            {
                options.ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat();
            }
        }

        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            var response = await _retryPipeline.ExecuteAsync(async ct => 
                await _chatClient.CompleteChatAsync(messages, options, ct), 
                cancellationToken);
                
            stopwatch.Stop();
            
            // Dummy cost calculation based on GPT-4o-mini rates
            var inputTokens = response.Value.Usage.InputTokenCount;
            var outputTokens = response.Value.Usage.OutputTokenCount;
            var estimatedCost = (inputTokens * 0.00015m / 1000m) + (outputTokens * 0.00060m / 1000m);
            
            return new AICompletionResponse
            {
                Content = response.Value.Content[0].Text,
                Usage = new AITokenUsage(inputTokens, outputTokens),
                ModelId = _modelId,
                ProviderName = "openai",
                PromptVersion = request.PromptVersion,
                Latency = stopwatch.Elapsed,
                EstimatedCost = estimatedCost
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI CompleteAsync failed.");
            return AICompletionResponse.Failure("openai", _modelId, request.PromptVersion, ex.Message);
        }
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _retryPipeline.ExecuteAsync(async ct => 
                await _embeddingClient.GenerateEmbeddingAsync(text, cancellationToken: ct), 
                cancellationToken);
                
            return response.Value.ToFloats().ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI EmbedAsync failed.");
            throw;
        }
    }
}
