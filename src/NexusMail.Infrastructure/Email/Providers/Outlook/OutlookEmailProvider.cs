using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Features.Email.Abstractions;

namespace NexusMail.Infrastructure.Email.Providers.Outlook;

/// <summary>
/// Microsoft Graph API provider stub for Outlook / Microsoft 365.
/// Will be implemented in Sprint 4 after Google provider is complete.
///
/// References:
///   - https://docs.microsoft.com/graph/api/resources/mail-api-overview
///   - OAuth2: https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token
/// </summary>
public sealed class OutlookEmailProvider : IEmailProvider
{
    private readonly ILogger<OutlookEmailProvider> _logger;

    public OutlookEmailProvider(ILogger<OutlookEmailProvider> logger)
    {
        _logger = logger;
    }

    public Task<ProviderProfile> GetProfileAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderProfile("outlook-user@domain.com", null));
    }

    public Task<IReadOnlyList<ProviderLabel>> GetLabelsAsync(string accessToken, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Outlook provider — Sprint 4.");

    public Task<ProviderMessagePage> GetMessagesAsync(
        string accessToken, string? pageToken = null, string? cursor = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Outlook provider — Sprint 4.");

    public Task<ProviderMessage> GetMessageAsync(string accessToken, string messageId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Outlook provider — Sprint 4.");

    public Task<ProviderTokens> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Outlook provider — Sprint 4.");
}
