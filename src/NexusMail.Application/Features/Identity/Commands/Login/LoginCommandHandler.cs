using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Application.Abstractions.Time;
using NexusMail.Application.Features.Identity.Results;
using NexusMail.Domain.Identity.Entities;
using NexusMail.Domain.Identity.Repositories;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Identity.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthenticationResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IClock _clock;
    private readonly Microsoft.Extensions.Logging.ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IUserSessionRepository sessionRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IClock clock,
        Microsoft.Extensions.Logging.ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result<AuthenticationResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("LoginFailed: {Email} - {Reason}", request.Email, "User not found");
            return Result.Failure<AuthenticationResult>(new Error("Identity.InvalidCredentials", "Invalid email or password."));
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("LoginFailed: {Email} - {Reason}", request.Email, "Invalid password");
            return Result.Failure<AuthenticationResult>(new Error("Identity.InvalidCredentials", "Invalid email or password."));
        }

        var tokens = _tokenService.GenerateTokens(user.Id, user.Email);
        
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(tokens.RefreshToken));
        var refreshTokenHash = Convert.ToBase64String(hashedBytes);

        var session = UserSession.Create(
            user.Id,
            refreshTokenHash,
            _clock.UtcNow,
            tokens.ExpiresAtUtc,
            request.DeviceName,
            request.IpAddress,
            request.UserAgent);

        await _sessionRepository.AddAsync(session, cancellationToken);

        _logger.LogInformation("LoginSuccess: UserId={UserId} SessionId={SessionId}", user.Id, session.Id);

        return Result.Success(tokens);
    }
}
