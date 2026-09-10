using System;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Domain.Email.Enums;
using NexusMail.Shared.Common;

namespace NexusMail.Infrastructure.Authentication;

public sealed class FakeOAuthProvider : IOAuthProvider
{
    public EmailProvider Provider => EmailProvider.Fake;

    public Task<Result<OAuthTokens>> ExchangeCodeAsync(string authCode, string redirectUri, CancellationToken cancellationToken = default)
    {
        // Simply return a successful fake token
        var tokens = new OAuthTokens(
            AccessToken: "fake-access-token",
            RefreshToken: "fake-refresh-token",
            ExpiresAt: DateTime.UtcNow.AddYears(1),
            EmailAddress: "user@fake-nexusmail.com"
        );
        return Task.FromResult(Result.Success(tokens));
    }

    public Task<Result<OAuthTokens>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokens = new OAuthTokens(
            AccessToken: "fake-access-token-refreshed",
            RefreshToken: "fake-refresh-token",
            ExpiresAt: DateTime.UtcNow.AddYears(1),
            EmailAddress: "user@fake-nexusmail.com"
        );
        return Task.FromResult(Result.Success(tokens));
    }
}
