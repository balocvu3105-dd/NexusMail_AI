using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Abstractions.Persistence;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Domain.Email.Enums;
using NexusMail.Shared.Common;

namespace NexusMail.Infrastructure.Authentication;

public sealed class EmailTokenService : IEmailTokenService
{
    private readonly IEmailAccountRepository _repository;
    private readonly ITokenEncryptionService _encryptionService;
    private readonly IEnumerable<IOAuthProvider> _oauthProviders;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmailTokenService> _logger;

    public EmailTokenService(
        IEmailAccountRepository repository,
        ITokenEncryptionService encryptionService,
        IEnumerable<IOAuthProvider> oauthProviders,
        IUnitOfWork unitOfWork,
        ILogger<EmailTokenService> logger)
    {
        _repository = repository;
        _encryptionService = encryptionService;
        _oauthProviders = oauthProviders;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<string> GetValidAccessTokenAsync(Guid emailAccountId, CancellationToken cancellationToken = default)
    {
        var account = await _repository.GetByIdAsync(emailAccountId, cancellationToken);
        if (account == null)
        {
            throw new Exception($"Email account {emailAccountId} not found.");
        }

        if (string.IsNullOrWhiteSpace(account.EncryptedAccessToken))
        {
            throw new Exception($"Email account {emailAccountId} has no access token.");
        }

        var accessToken = await _encryptionService.DecryptAsync(account.EncryptedAccessToken, account.EncryptionVersion, cancellationToken);

        // If the token is valid for at least another 5 minutes, return it
        if (account.TokenExpiresAt.HasValue && account.TokenExpiresAt.Value > DateTime.UtcNow.AddMinutes(5))
        {
            return accessToken;
        }

        _logger.LogInformation("Token for account {AccountId} is expired or expiring soon. Attempting refresh.", emailAccountId);

        if (string.IsNullOrWhiteSpace(account.EncryptedRefreshToken))
        {
            throw new Exception($"Cannot refresh token for account {emailAccountId} because it has no refresh token.");
        }

        var refreshToken = await _encryptionService.DecryptAsync(account.EncryptedRefreshToken, account.EncryptionVersion, cancellationToken);
        
        var provider = _oauthProviders.FirstOrDefault(p => p.Provider == account.Provider);
        if (provider == null)
        {
            throw new Exception($"OAuth provider {account.Provider} is not supported.");
        }

        var refreshResult = await provider.RefreshTokenAsync(refreshToken, cancellationToken);
        if (refreshResult.IsFailure)
        {
            _logger.LogError("Failed to refresh token for account {AccountId}: {Error}", emailAccountId, refreshResult.Error.Description);
            account.MarkAsError();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new Exception($"Failed to refresh token: {refreshResult.Error.Description}");
        }

        var newTokens = refreshResult.Value;
        
        var newEncryptedAccess = await _encryptionService.EncryptAsync(newTokens.AccessToken, cancellationToken);
        var newEncryptedRefresh = await _encryptionService.EncryptAsync(newTokens.RefreshToken, cancellationToken);

        account.UpdateTokens(newEncryptedAccess, newEncryptedRefresh, newTokens.ExpiresAt, _encryptionService.CurrentVersion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully refreshed token for account {AccountId}.", emailAccountId);

        return newTokens.AccessToken;
    }
}
