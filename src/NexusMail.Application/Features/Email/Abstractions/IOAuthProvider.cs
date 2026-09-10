using System.Threading;
using System.Threading.Tasks;
using NexusMail.Domain.Email.Enums;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Email.Abstractions;

public record OAuthTokens(
    string AccessToken,
    string RefreshToken,
    System.DateTime? ExpiresAt,
    string EmailAddress);

public interface IOAuthProvider
{
    EmailProvider Provider { get; }
    
    Task<Result<OAuthTokens>> ExchangeCodeAsync(string authCode, string redirectUri, CancellationToken cancellationToken = default);
    Task<Result<OAuthTokens>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
