using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexusMail.Domain.Identity.Entities;
using NexusMail.Domain.Identity.Repositories;

namespace NexusMail.Infrastructure.Persistence.Repositories;

public sealed class UserSessionRepository : IUserSessionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserSessionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserSessions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<UserSession?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserSessions.FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshTokenHash, cancellationToken);
    }

    public Task AddAsync(UserSession session, CancellationToken cancellationToken = default)
    {
        _dbContext.UserSessions.Add(session);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(UserSession session, CancellationToken cancellationToken = default)
    {
        _dbContext.UserSessions.Update(session);
        return Task.CompletedTask;
    }
}
