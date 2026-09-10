using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Features.Email.Abstractions;

namespace NexusMail.Infrastructure.Email.Providers.Imap;

/// <summary>
/// IMAP/SMTP provider stub for generic email accounts.
/// Planned: MailKit library for IMAP RFC 3501 compliance.
/// Will be implemented in Sprint 4+ after Google and Outlook providers.
///
/// References:
///   - MailKit: https://github.com/jstedfast/MailKit
///   - IMAP RFC 3501: https://tools.ietf.org/html/rfc3501
/// </summary>
public sealed class ImapEmailProvider : IEmailProvider
{
    private readonly ILogger<ImapEmailProvider> _logger;

    public ImapEmailProvider(ILogger<ImapEmailProvider> logger)
    {
        _logger = logger;
    }

    public Task<ProviderProfile> GetProfileAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderProfile("imap-user@domain.com", null));
    }

    public Task<IReadOnlyList<ProviderLabel>> GetLabelsAsync(string accessToken, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("IMAP provider — Sprint 4+.");

    public Task<ProviderMessagePage> GetMessagesAsync(
        string accessToken, string? pageToken = null, string? cursor = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("IMAP provider — Sprint 4+.");

    public Task<ProviderMessage> GetMessageAsync(string accessToken, string messageId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("IMAP provider — Sprint 4+.");

    public Task<ProviderTokens> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("IMAP provider — Sprint 4+.");
}
