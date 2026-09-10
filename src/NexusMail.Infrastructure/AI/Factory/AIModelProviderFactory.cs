using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NexusMail.Application.Abstractions.AI;

namespace NexusMail.Infrastructure.AI.Factory;

public sealed class AIModelProviderFactory : IAIModelProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _defaultProviderName;

    public AIModelProviderFactory(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _defaultProviderName = configuration["AI:DefaultProvider"] ?? "openai";
    }

    public IAIModelProvider GetDefault()
    {
        return Get(_defaultProviderName);
    }

    public IAIModelProvider Get(string providerName)
    {
        var provider = _serviceProvider.GetKeyedService<IAIModelProvider>(providerName);
        if (provider == null)
            throw new InvalidOperationException($"AI model provider '{providerName}' is not registered.");
            
        return provider;
    }

    public IAIModelProvider GetForEmbedding()
    {
        // Check if the default provider supports embedding, otherwise throw
        var defaultProvider = GetDefault();
        if (defaultProvider.SupportsEmbedding)
            return defaultProvider;
            
        throw new NotSupportedException($"The default provider '{_defaultProviderName}' does not support embeddings.");
    }

    public IReadOnlyList<string> GetRegisteredProviders()
    {
        // Hardcoded list for now, dynamically resolving keyed service keys requires .NET 8+ advanced reflection
        // but since we control the registration, this is acceptable.
        return new[] { "openai", "claude", "gemini", "ollama" };
    }
}
