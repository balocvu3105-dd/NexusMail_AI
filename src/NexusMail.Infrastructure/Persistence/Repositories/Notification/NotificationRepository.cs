using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexusMail.Notification.Domain;

namespace NexusMail.Infrastructure.Persistence.Repositories.Notification;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public NotificationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(NotificationItem notification, CancellationToken cancellationToken = default)
    {
        await _dbContext.Notifications.AddAsync(notification, cancellationToken);
    }

    public async Task<NotificationItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationItem>> GetByWorkspaceAsync(Guid workspaceId, bool unreadOnly, int limit, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Notifications.Where(n => n.WorkspaceId == workspaceId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAllAsReadAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        await _dbContext.Notifications
            .Where(n => n.WorkspaceId == workspaceId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken);
    }
}
