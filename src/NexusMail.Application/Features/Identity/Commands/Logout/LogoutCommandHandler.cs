using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Abstractions.Time;
using NexusMail.Domain.Identity.Repositories;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Identity.Commands.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IClock _clock;

    public LogoutCommandHandler(IUserSessionRepository sessionRepository, IClock clock)
    {
        _sessionRepository = sessionRepository;
        _clock = clock;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(request.RefreshToken));
        var refreshTokenHash = Convert.ToBase64String(hashedBytes);

        var session = await _sessionRepository.GetByRefreshTokenHashAsync(refreshTokenHash, cancellationToken);

        if (session != null && session.IsActive(_clock.UtcNow))
        {
            session.Revoke(_clock.UtcNow);
            await _sessionRepository.UpdateAsync(session, cancellationToken);
        }

        // We return success even if session is not found or already revoked (idempotent)
        return Result.Success();
    }
}
