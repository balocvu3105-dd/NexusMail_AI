using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Abstractions.AI;
using NexusMail.Application.Abstractions.Copilot;
using NexusMail.Shared.Common;

namespace NexusMail.Infrastructure.AI.Services;

public sealed class CopilotLanguageService : ICopilotLanguageService
{
    private readonly IAIModelProvider _provider;
    private readonly ILogger<CopilotLanguageService> _logger;

    public CopilotLanguageService(
        IAIModelProviderFactory providerFactory,
        IConfiguration configuration,
        ILogger<CopilotLanguageService> logger)
    {
        var defaultProvider = configuration["AI:DefaultProvider"] ?? "openai";
        _provider = providerFactory.Get(defaultProvider);
        _logger = logger;
    }

    public async Task<LLMSynthesisResult> SynthesizeAnswerAsync(string query, string contextDocument, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Synthesizing Copilot answer via {ProviderName}", _provider.ProviderName);

        var systemPrompt = @"You are NexusMail Copilot, a helpful email assistant.
You will be provided with a query and a set of email contexts.
Answer the user's query ONLY using the provided context.
If the user's query implies taking an action (e.g., 'Create a task', 'Label this email'), you MUST populate 'suggestedActions' with the appropriate action ('CreateTask' or 'ApplyLabel') and its parameters.
If the context does not contain enough information to answer the query, set the state to 'InsufficientEvidence' and explain why.
Otherwise, set the state to 'Grounded' and provide the answer.
Output ONLY valid JSON matching the exact schema provided. Do not include markdown code blocks. Do not invent or hallucinate information.";

        var content = $"QUERY:\n{query}\n\nCONTEXT:\n{contextDocument}";

        var jsonSchema = """
        {
          "name": "copilot_synthesis_result",
          "strict": true,
          "schema": {
            "type": "object",
            "properties": {
              "answer": { "type": "string" },
              "state": { "type": "string", "enum": ["Grounded", "InsufficientEvidence"] },
              "suggestedActions": {
                "type": "array",
                "items": {
                  "type": "object",
                  "properties": {
                    "actionType": { "type": "string", "enum": ["CreateTask", "ApplyLabel"] },
                    "parameters": {
                      "type": "object",
                      "properties": {
                        "title": { "type": ["string", "null"] },
                        "description": { "type": ["string", "null"] },
                        "dueAt": { "type": ["string", "null"] },
                        "emailId": { "type": ["string", "null"] },
                        "label": { "type": ["string", "null"] }
                      },
                      "additionalProperties": false
                    }
                  },
                  "required": ["actionType", "parameters"],
                  "additionalProperties": false
                }
              }
            },
            "required": ["answer", "state", "suggestedActions"],
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
            PromptVersion = "Copilot_v2"
        };

        try
        {
            var response = await _provider.CompleteAsync(request, cancellationToken);
            if (!response.IsSuccess)
            {
                // Error propagating up, Execution Failure != Unknown
                throw new InvalidOperationException($"AI Provider failed: {response.ErrorMessage}");
            }

            var result = JsonSerializer.Deserialize<CopilotJsonResult>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (result == null || string.IsNullOrWhiteSpace(result.State))
            {
                throw new InvalidOperationException("Failed to parse synthesis JSON or missing required fields.");
            }

            if (!Enum.TryParse<GroundingState>(result.State, true, out var groundingState))
            {
                throw new InvalidOperationException($"Invalid grounding state returned by LLM: {result.State}");
            }

            return new LLMSynthesisResult
            {
                Answer = result.Answer,
                State = groundingState,
                SuggestedActions = result.SuggestedActions ?? new List<ActionProposal>()
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse synthesis JSON");
            throw new InvalidOperationException("AI Synthesis parsing failed", ex);
        }
    }

    public async IAsyncEnumerable<LanguageStreamChunk> SynthesizeStreamAsync(string query, string contextDocument, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Synthesizing Copilot answer stream via {ProviderName}", _provider.ProviderName);

        var systemPrompt = @"You are NexusMail Copilot, a helpful email assistant.
You will be provided with a query and a set of email contexts.
Answer the user's query ONLY using the provided context.
If the user's query implies taking an action (e.g., 'Create a task', 'Label this email'), you MUST populate 'suggestedActions' with the appropriate action ('CreateTask' or 'ApplyLabel') and its parameters.
If the context does not contain enough information to answer the query, set the state to 'InsufficientEvidence' and explain why.
Otherwise, set the state to 'Grounded' and provide the answer.
Output ONLY valid JSON matching the exact schema provided. Do not include markdown code blocks. Do not invent or hallucinate information.";

        var content = $"QUERY:\n{query}\n\nCONTEXT:\n{contextDocument}";

        var jsonSchema = """
        {
          "name": "copilot_synthesis_result",
          "strict": true,
          "schema": {
            "type": "object",
            "properties": {
              "answer": { "type": "string" },
              "state": { "type": "string", "enum": ["Grounded", "InsufficientEvidence"] },
              "suggestedActions": {
                "type": "array",
                "items": {
                  "type": "object",
                  "properties": {
                    "actionType": { "type": "string", "enum": ["CreateTask", "ApplyLabel"] },
                    "parameters": {
                      "type": "object",
                      "properties": {
                        "title": { "type": ["string", "null"] },
                        "description": { "type": ["string", "null"] },
                        "dueAt": { "type": ["string", "null"] },
                        "emailId": { "type": ["string", "null"] },
                        "label": { "type": ["string", "null"] }
                      },
                      "additionalProperties": false
                    }
                  },
                  "required": ["actionType", "parameters"],
                  "additionalProperties": false
                }
              }
            },
            "required": ["answer", "state", "suggestedActions"],
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
            PromptVersion = "Copilot_v2"
        };

        var parser = new IncrementalAnswerParser();
        var fullJsonResponseBuilder = new System.Text.StringBuilder();

        var stream = _provider.CompleteStreamAsync(request, cancellationToken);
        await foreach (var chunk in stream)
        {
            fullJsonResponseBuilder.Append(chunk);
            var parsedChunks = parser.ParseChunk(chunk);
            foreach (var parsedChunk in parsedChunks)
            {
                yield return new LanguageStreamChunk(parsedChunk, null, null);
            }
        }

        var fullJson = fullJsonResponseBuilder.ToString();
        CopilotJsonResult? result = null;
        try
        {
            result = JsonSerializer.Deserialize<CopilotJsonResult>(fullJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse full synthesis JSON stream: {Json}", fullJson);
            throw new InvalidOperationException("AI Synthesis parsing failed", ex);
        }
        
        if (result == null || string.IsNullOrWhiteSpace(result.State))
        {
            throw new InvalidOperationException("Failed to parse synthesis JSON or missing required fields.");
        }

        if (!Enum.TryParse<GroundingState>(result.State, true, out var groundingState))
        {
            throw new InvalidOperationException($"Invalid grounding state returned by LLM: {result.State}");
        }

        var actions = result.SuggestedActions ?? new List<ActionProposal>();

        yield return new LanguageStreamChunk(null, groundingState, actions);
    }

    private class CopilotJsonResult
    {
        public string Answer { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public List<ActionProposal>? SuggestedActions { get; set; }
    }
}
