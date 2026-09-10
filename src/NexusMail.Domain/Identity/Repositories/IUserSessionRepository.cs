using System;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Domain.Identity.Entities;

namespace NexusMail.Domain.Identity.Repositories;

public interface IUserSessionRepository
{
    Task<UserSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserSession?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default);
    Task AddAsync(UserSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserSession session, CancellationToken cancellationToken = default);
}
