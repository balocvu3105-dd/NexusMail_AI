using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Domain.Email.Enums;

namespace NexusMail.Application.Features.Email.Abstractions;

/// <summary>
/// Factory that resolves the correct IEmailProvider implementation
/// based on the EmailProviderType of an account.
///
/// DESIGN: Application code never instantiates providers directly.
/// This breaks the dependency on specific provider implementations.
///
/// Usage:
///   var provider = _factory.GetProvider(account.Provider);
///   var messages = await provider.GetMessagesAsync(token, null, ct);
/// </summary>
public interface IEmailProviderFactory
{
    /// <summary>Returns the IEmailProvider for the given provider type.</summary>
    /// <exception cref="NotSupportedException">Thrown when no provider is registered for the type.</exception>
    IEmailProvider GetProvider(EmailProvider providerType);

    /// <summary>Returns true if a provider is registered for the given type.</summary>
    bool IsSupported(EmailProvider providerType);
}
