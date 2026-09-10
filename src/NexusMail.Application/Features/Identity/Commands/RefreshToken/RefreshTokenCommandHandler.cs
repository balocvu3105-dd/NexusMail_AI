using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Application.Abstractions.Time;
using NexusMail.Application.Features.Identity.Results;
using NexusMail.Domain.Identity.Repositories;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Identity.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthenticationResult>>
{
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IClock _clock;
    private readonly Microsoft.Extensions.Logging.ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUserSessionRepository sessionRepository,
        IUserRepository userRepository,
        ITokenService tokenService,
        IClock clock,
        Microsoft.Extensions.Logging.ILogger<RefreshTokenCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result<AuthenticationResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(request.RefreshToken));
        var refreshTokenHash = Convert.ToBase64String(hashedBytes);

        var session = await _sessionRepository.GetByRefreshTokenHashAsync(refreshTokenHash, cancellationToken);

        if (session == null)
        {
            _logger.LogWarning("RefreshTokenRejected: SessionId={SessionId} - {Reason}", "Unknown", "Invalid refresh token");
            return Result.Failure<AuthenticationResult>(new Error("Identity.InvalidToken", "Invalid refresh token."));
        }

        if (!session.IsActive(_clock.UtcNow))
        {
            _logger.LogWarning("RefreshTokenRejected: SessionId={SessionId} - {Reason}", session.Id, "Token expired or revoked");
            return Result.Failure<AuthenticationResult>(new Error("Identity.InvalidToken", "Refresh token has expired or been revoked."));
        }

        var user = await _userRepository.GetByIdAsync(session.UserId, cancellationToken);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("RefreshTokenRejected: SessionId={SessionId} - {Reason}", session.Id, "User inactive or deleted");
            return Result.Failure<AuthenticationResult>(new Error("Identity.UserInactive", "User is inactive or deleted."));
        }

        var tokens = _tokenService.GenerateTokens(user.Id, user.Email);
        
        var newHashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(tokens.RefreshToken));
        var newRefreshTokenHash = Convert.ToBase64String(newHashedBytes);

        session.Rotate(newRefreshTokenHash, tokens.ExpiresAtUtc);

        await _sessionRepository.UpdateAsync(session, cancellationToken);
        
        _logger.LogInformation("RefreshTokenSuccess: UserId={UserId} SessionId={SessionId}", user.Id, session.Id);

        return Result.Success(tokens);
    }
}
