using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Application.Features.Notification.DTOs;
using NexusMail.Notification.Domain;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.Notification.Queries;

public sealed class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<IReadOnlyList<NotificationDto>>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IWorkspaceContext _workspaceContext;

    public GetNotificationsQueryHandler(
        INotificationRepository notificationRepository, 
        IWorkspaceContext workspaceContext)
    {
        _notificationRepository = notificationRepository;
        _workspaceContext = workspaceContext;
    }

    public async Task<Result<IReadOnlyList<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        if (_workspaceContext.WorkspaceId == null)
        {
            return Result.Failure<IReadOnlyList<NotificationDto>>(Error.NullValue);
        }

        var limit = System.Math.Min(request.Limit, 100);

        var notifications = await _notificationRepository.GetByWorkspaceAsync(
            _workspaceContext.WorkspaceId.Value, 
            request.UnreadOnly, 
            limit, 
            cancellationToken);

        var dtos = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Type = n.Type.ToString(),
            Title = n.Title,
            Message = n.Message,
            ActionUrl = n.ActionUrl,
            Payload = string.IsNullOrWhiteSpace(n.PayloadJson) ? null : JsonDocument.Parse(n.PayloadJson),
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();

        return Result.Success<IReadOnlyList<NotificationDto>>(dtos);
    }
}
