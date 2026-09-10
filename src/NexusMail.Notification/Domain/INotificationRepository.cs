using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NexusMail.Notification.Domain;

public interface INotificationRepository
{
    Task AddAsync(NotificationItem notification, CancellationToken cancellationToken = default);
    Task<NotificationItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationItem>> GetByWorkspaceAsync(Guid workspaceId, bool unreadOnly, int limit, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid workspaceId, CancellationToken cancellationToken = default);
}
