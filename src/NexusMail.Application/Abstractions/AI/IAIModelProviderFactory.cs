namespace NexusMail.Application.Abstractions.AI;

/// <summary>
/// Resolves the appropriate AI model provider by name or capability.
///
/// DESIGN: Application configures which provider to use via configuration,
/// not by hard-coding. Switch from OpenAI to Claude = config change only.
///
/// Usage:
///   var provider = _factory.GetDefault();             // from config
///   var provider = _factory.Get("claude");            // by name
///   var provider = _factory.GetForEmbedding();        // provider that supports embeddings
/// </summary>
public interface IAIModelProviderFactory
{
    /// <summary>Returns the default provider as configured (e.g., "openai").</summary>
    IAIModelProvider GetDefault();

    /// <summary>Returns a provider by name. Throws if not registered.</summary>
    IAIModelProvider Get(string providerName);

    /// <summary>Returns the configured provider for embedding generation.</summary>
    IAIModelProvider GetForEmbedding();

    /// <summary>Returns all registered provider names.</summary>
    IReadOnlyList<string> GetRegisteredProviders();
}
