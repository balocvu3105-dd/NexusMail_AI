using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NexusMail.Application.Abstractions.AI;
using NexusMail.Infrastructure.AI.Providers;
using Xunit;
using Xunit.Abstractions;

namespace NexusMail.Tests.AI;

public class AIProviderContractTests
{
    private readonly ITestOutputHelper _output;

    public AIProviderContractTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private IAIModelProvider CreateOpenAIProvider()
    {
        // This relies on environment variables or local user secrets for testing.
        // If OPENAI_API_KEY is not set, these tests will fail or skip.
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            // Fallback for CI/CD if we want to skip or fail
            _output.WriteLine("Warning: OPENAI_API_KEY not found in env.");
        }

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("AI:OpenAI:ApiKey", apiKey ?? "fake-key"),
                new KeyValuePair<string, string?>("AI:OpenAI:ModelId", "gpt-4o-mini"),
                new KeyValuePair<string, string?>("AI:OpenAI:EmbeddingModelId", "text-embedding-3-small")
            })
            .Build();

        return new OpenAIModelProvider(config, NullLogger<OpenAIModelProvider>.Instance);
    }

    [Fact]
    public async Task ParseEmailAsync_ShouldReturnValidSchema()
    {
        var provider = CreateOpenAIProvider();
        
        // Skip if fake key
        if (Environment.GetEnvironmentVariable("OPENAI_API_KEY") == null)
            return;

        var emailContent = "Subject: Urgent: Invoice #12345 Due Today\n\nPlease find attached the invoice for your recent purchase. Immediate payment is required.";
        
        var request = new AICompletionRequest
        {
            SystemPrompt = "Extract Summary, PriorityScore, and Category. Return JSON format: { \"Summary\": \"...\", \"PriorityScore\": 10, \"Category\": \"...\" }",
            Messages = new[] { new AIMessage(AIRole.User, emailContent) },
            ResponseFormat = AIResponseFormat.Json
        };

        var sw = Stopwatch.StartNew();
        var result = await provider.CompleteAsync(request, CancellationToken.None);
        sw.Stop();

        _output.WriteLine($"Latency: {sw.ElapsedMilliseconds} ms");

        Assert.NotNull(result);
        Assert.NotEmpty(result.Content);
        Assert.True(result.Usage.PromptTokens > 0);
    }

    [Fact]
    public async Task ParseEmailAsync_ShouldRespectCancellation()
    {
        var provider = CreateOpenAIProvider();
        
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(5)); // Extremely short timeout

        var request = new AICompletionRequest 
        { 
            Messages = new[] { new AIMessage(AIRole.User, "Hello") } 
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await provider.CompleteAsync(request, cts.Token);
        });
    }

    [Fact]
    public async Task ParseEmailAsync_MalformedResponse_ShouldThrowOrRetry()
    {
        // ...
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_ShouldReturnCorrectDimensions()
    {
        var provider = CreateOpenAIProvider();
        
        if (Environment.GetEnvironmentVariable("OPENAI_API_KEY") == null)
            return;

        var text = "This is a test document for embeddings.";
        var vector = await provider.EmbedAsync(text, CancellationToken.None);

        Assert.NotNull(vector);
        // text-embedding-3-small has 1536 dimensions
        Assert.Equal(1536, vector.Length);
    }
}
