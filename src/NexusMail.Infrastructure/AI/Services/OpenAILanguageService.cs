using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Abstractions.AI;
using NexusMail.Shared.Common;

namespace NexusMail.Infrastructure.AI.Services;

public sealed class OpenAILanguageService : IAIProcessingService, IEmbeddingService
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

    public async Task<Result<EmailAnalysisResult>> ProcessEmailAsync(string subject, string body, string sender, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing email via {ProviderName}", _provider.ProviderName);

        var systemPrompt = "Analyze this email. Output ONLY valid JSON matching the exact schema provided. Do not include markdown code blocks. Provide a concise summary, priority score (1-100), classification, and whether it requires immediate human attention.";
        var truncatedBody = body.Length > 12_000 ? body[..12_000] : body;
        var content = $"From: {sender}\nSubject: {subject}\n\n{truncatedBody}";

        var jsonSchema = """
        {
          "name": "email_analysis_result",
          "strict": true,
          "schema": {
            "type": "object",
            "properties": {
              "classification": {
                "type": "object",
                "properties": {
                  "category": { "type": "string", "enum": ["Spam", "Newsletter", "Personal", "Work", "Alert", "Other"] },
                  "confidence": { "type": "number", "minimum": 0, "maximum": 1 },
                  "tags": { "type": "array", "items": { "type": "string" } }
                },
                "required": ["category", "confidence", "tags"],
                "additionalProperties": false
              },
              "priority": {
                "type": "object",
                "properties": {
                  "score": { "type": "integer", "minimum": 1, "maximum": 100 },
                  "reason": { "type": "string" }
                },
                "required": ["score", "reason"],
                "additionalProperties": false
              },
              "summary": { "type": "string" },
              "needsAttention": {
                "type": "boolean",
                "description": "True if the email requires immediate human action or reply."
              }
            },
            "required": ["classification", "priority", "summary", "needsAttention"],
            "additionalProperties": false
          }
        }
        """;

        var request = new AICompletionRequest
        {
            SystemPrompt = systemPrompt,
            Messages = new[] { new AIMessage(AIRole.User, content) },
            Temperature = 0.1,
            ResponseFormat = AIResponseFormat.Json,
            JsonSchema = jsonSchema,
            PromptVersion = "Processing_v1"
        };

        try
        {
            var response = await _provider.CompleteAsync(request, cancellationToken);
            if (!response.IsSuccess)
            {
                return HandleProviderFailure<EmailAnalysisResult>(response.ErrorMessage);
            }

            var result = JsonSerializer.Deserialize<ProcessingJsonResult>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (result == null || result.Classification == null || result.Priority == null)
            {
                return Result.Failure<EmailAnalysisResult>(new Error("AI.InvalidResponse", "Failed to parse processing JSON or missing required fields."));
            }

            return Result.Success(new EmailAnalysisResult(
                result.Summary,
                result.Priority.Score,
                result.Priority.Reason,
                result.Classification.Category,
                result.Classification.Confidence,
                result.Classification.Tags,
                result.NeedsAttention
            ));
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse processing JSON");
            return Result.Failure<EmailAnalysisResult>(new Error("AI.InvalidFormat", "AI Processing parsing failed"));
        }
        catch (Exception ex)
        {
            return HandleException<EmailAnalysisResult>(ex);
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
        
        var message = ex.Message.ToLowerInvariant();
        if (message.Contains("429") || message.Contains("503") || message.Contains("timeout") || message.Contains("transient"))
        {
            throw new AIProviderTransientException($"Transient AI execution error: {ex.Message}", ex);
        }

        _logger.LogError(ex, "Permanent AI execution error");
        return Result.Failure<T>(new Error("AI.ExecutionError", $"AI Execution failed: {ex.Message}"));
    }

    // Private DTOs for JSON deserialization
    private class ProcessingJsonResult
    {
        public ClassificationJsonResult Classification { get; set; } = new();
        public PriorityJsonResult Priority { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
        public bool NeedsAttention { get; set; }
    }

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
    }
}
