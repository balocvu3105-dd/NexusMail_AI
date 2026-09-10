using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Abstractions.AI;
using NexusMail.Shared.Common;

namespace NexusMail.Infrastructure.AI.Services;

public sealed class OpenAILanguageService : ISummaryService, IPriorityService, IClassificationService, IEmbeddingService
{
    private readonly IAIModelProvider _provider;
    private readonly ILogger<OpenAILanguageService> _logger;

    public OpenAILanguageService(
        IAIModelProviderFactory providerFactory,
        IConfiguration configuration,
        ILogger<OpenAILanguageService> logger)
    {
        var defaultProvider = configuration["AI:DefaultProvider"] ?? "openai";
        _provider = providerFactory.Get(defaultProvider);
        _logger = logger;
    }

    public async Task<Result<string>> GenerateSummaryAsync(string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating summary via {ProviderName}", _provider.ProviderName);

        var prompt = "Summarize this email in 2-5 concise sentences. Focus on the core intent, requested actions, and key facts. Be objective.";
        var content = $"Subject: {subject}\n\n{body}";

        var request = new AICompletionRequest
        {
            SystemPrompt = prompt,
            Messages = new[] { new AIMessage(AIRole.User, content) },
            Temperature = 0.2,
            MaxTokens = 250,
            PromptVersion = "Summary_v1"
        };

        try
        {
            var response = await _provider.CompleteAsync(request, cancellationToken);
            if (!response.IsSuccess)
            {
                return HandleProviderFailure<string>(response.ErrorMessage);
            }
            return Result.Success(response.Content.Trim());
        }
        catch (Exception ex)
        {
            return HandleException<string>(ex);
        }
    }

    public async Task<Result<AIClassificationResult>> GenerateClassificationAsync(string subject, string body, string sender, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Classifying email via {ProviderName}", _provider.ProviderName);

        var systemPrompt = "Classify this email. Output ONLY valid JSON matching the exact schema provided. Do not include markdown code blocks.";
        var content = $"From: {sender}\nSubject: {subject}\n\n{body}";

        var jsonSchema = """
        {
            "name": "classification_result",
            "schema": {
                "type": "object",
                "properties": {
                    "category": { "type": "string", "description": "The primary category of the email (e.g. Invoice, Newsletter, Support, Personal, Alert)" },
                    "confidence": { "type": "number", "description": "Confidence score between 0.0 and 1.0" },
                    "tags": { "type": "array", "items": { "type": "string" }, "description": "1 to 5 relevant tags/keywords" }
                },
                "required": ["category", "confidence", "tags"],
                "additionalProperties": false
            },
            "strict": true
        }
        """;

        var request = new AICompletionRequest
        {
            SystemPrompt = systemPrompt,
            Messages = new[] { new AIMessage(AIRole.User, content) },
            Temperature = 0.1,
            ResponseFormat = AIResponseFormat.Json,
            JsonSchema = jsonSchema,
            PromptVersion = "Classification_v1"
        };

        try
        {
            var response = await _provider.CompleteAsync(request, cancellationToken);
            if (!response.IsSuccess)
            {
                return HandleProviderFailure<AIClassificationResult>(response.ErrorMessage);
            }

            var result = JsonSerializer.Deserialize<ClassificationJsonResult>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (result == null || string.IsNullOrWhiteSpace(result.Category))
            {
                return Result.Failure<AIClassificationResult>(new Error("AI.InvalidResponse", "Failed to parse classification JSON or missing required fields."));
            }

            return Result.Success(new AIClassificationResult(result.Category, result.Confidence, result.Tags));
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse classification JSON");
            return Result.Failure<AIClassificationResult>(new Error("AI.InvalidFormat", "AI Classification parsing failed"));
        }
        catch (Exception ex)
        {
            return HandleException<AIClassificationResult>(ex);
        }
    }

    public async Task<Result<AIPriorityResult>> GeneratePriorityAsync(string subject, string body, string sender, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Assessing priority via {ProviderName}", _provider.ProviderName);

        var systemPrompt = "Assess the priority of this email (1 = lowest, 5 = highest). Output ONLY valid JSON.";
        var content = $"From: {sender}\nSubject: {subject}\n\n{body}";

        var jsonSchema = """
        {
            "name": "priority_result",
            "schema": {
                "type": "object",
                "properties": {
                    "score": { "type": "integer", "description": "Priority score from 1 to 5" },
                    "reason": { "type": "string", "description": "Short explanation of why this score was given" },
                    "requiresAction": { "type": "boolean", "description": "True if the user needs to reply or take action" }
                },
                "required": ["score", "reason", "requiresAction"],
                "additionalProperties": false
            },
            "strict": true
        }
        """;

        var request = new AICompletionRequest
        {
            SystemPrompt = systemPrompt,
            Messages = new[] { new AIMessage(AIRole.User, content) },
            Temperature = 0.1,
            ResponseFormat = AIResponseFormat.Json,
            JsonSchema = jsonSchema,
            PromptVersion = "Priority_v1"
        };

        try
        {
            var response = await _provider.CompleteAsync(request, cancellationToken);
            if (!response.IsSuccess)
            {
                return HandleProviderFailure<AIPriorityResult>(response.ErrorMessage);
            }

            var result = JsonSerializer.Deserialize<PriorityJsonResult>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (result == null)
            {
                return Result.Failure<AIPriorityResult>(new Error("AI.InvalidResponse", "Failed to parse priority JSON"));
            }

            return Result.Success(new AIPriorityResult(result.Score, result.Reason, result.RequiresAction));
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse priority JSON");
            return Result.Failure<AIPriorityResult>(new Error("AI.InvalidFormat", "AI Priority parsing failed"));
        }
        catch (Exception ex)
        {
            return HandleException<AIPriorityResult>(ex);
        }
    }

    public async Task<Result<float[]>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (!_provider.SupportsEmbedding)
            return Result.Failure<float[]>(new Error("AI.NotSupported", $"Provider {_provider.ProviderName} does not support embeddings."));

        _logger.LogInformation("Generating embedding via {ProviderName}", _provider.ProviderName);
        try
        {
            var embedding = await _provider.EmbedAsync(text, cancellationToken);
            return Result.Success(embedding);
        }
        catch (Exception ex)
        {
            return HandleException<float[]>(ex);
        }
    }

    private Result<T> HandleProviderFailure<T>(string errorMessage)
    {
        // For testing, simulate specific failure types based on message
        if (errorMessage.Contains("429") || errorMessage.Contains("503") || errorMessage.Contains("timeout") || errorMessage.Contains("Transient"))
        {
            throw new AIProviderTransientException($"Transient provider failure: {errorMessage}");
        }
        
        // Treat as permanent by default
        return Result.Failure<T>(new Error("AI.ProviderFailure", $"AI Provider failed: {errorMessage}"));
    }

    private Result<T> HandleException<T>(Exception ex)
    {
        if (ex is AIProviderTransientException)
        {
            throw ex; // Re-throw transient exceptions
        }
        
        // For integration testing or real implementations that throw HttpRequestException, etc.
        var message = ex.Message.ToLowerInvariant();
        if (message.Contains("429") || message.Contains("503") || message.Contains("timeout") || message.Contains("transient"))
        {
            throw new AIProviderTransientException($"Transient AI execution error: {ex.Message}", ex);
        }

        _logger.LogError(ex, "Permanent AI execution error");
        return Result.Failure<T>(new Error("AI.ExecutionError", $"AI Execution failed: {ex.Message}"));
    }

    // Private DTOs for JSON deserialization
    private class ClassificationJsonResult
    {
        public string Category { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
    }

    private class PriorityJsonResult
    {
        public int Score { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool RequiresAction { get; set; }
    }
}
