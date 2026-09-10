using Microsoft.Extensions.DependencyInjection;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Domain.Email.Enums;
using NexusMail.Infrastructure.Email.Providers.Google;
using NexusMail.Infrastructure.Email.Providers.Outlook;
using NexusMail.Infrastructure.Email.Providers.Imap;

namespace NexusMail.Infrastructure.Email.Factory;

/// <summary>
/// Resolves IEmailProvider by EmailProvider enum value.
///
/// DESIGN: Uses keyed DI (.NET 8+) via IServiceProvider.
/// Adding a new provider = register it with a key in DI — no changes here.
///
/// To add a new provider (e.g., Yahoo):
///   1. Create Infrastructure/Email/Providers/Yahoo/YahooEmailProvider.cs
///   2. Register: services.AddKeyedScoped&lt;IEmailProvider, YahooEmailProvider&gt;(EmailProvider.Yahoo)
///   3. Done — factory auto-resolves it.
/// </summary>
public sealed class EmailProviderFactory : IEmailProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public EmailProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEmailProvider GetProvider(EmailProvider providerType)
    {
        if (!IsSupported(providerType))
            throw new NotSupportedException(
                $"Email provider '{providerType}' is not supported. " +
                $"Supported: {string.Join(", ", SupportedProviders)}");

        // Resolve via keyed DI (.NET 8+)
        var provider = _serviceProvider.GetKeyedService<IEmailProvider>(providerType)
            ?? throw new InvalidOperationException(
                $"No IEmailProvider registered for key '{providerType}'. " +
                $"Ensure it is registered in DependencyInjection.cs.");

        return provider;
    }

    public bool IsSupported(EmailProvider providerType)
        => SupportedProviders.Contains(providerType);

    private static readonly HashSet<EmailProvider> SupportedProviders =
    [
        EmailProvider.Google,
        EmailProvider.Outlook,
        EmailProvider.Microsoft365,
        EmailProvider.IMAP,
        EmailProvider.Fake,
    ];
}
