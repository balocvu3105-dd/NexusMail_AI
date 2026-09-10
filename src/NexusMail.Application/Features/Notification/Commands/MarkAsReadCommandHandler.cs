using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Application.Abstractions.Persistence;
using NexusMail.Notification.Domain;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.Notification.Commands;

public sealed class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, Result>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IWorkspaceContext _workspaceContext;

    public MarkAsReadCommandHandler(
        INotificationRepository notificationRepository, 
        IWorkspaceContext workspaceContext)
    {
        _notificationRepository = notificationRepository;
        _workspaceContext = workspaceContext;
    }

    public async Task<Result> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        if (_workspaceContext.WorkspaceId == null)
        {
            return Result.Failure(Error.NullValue);
        }

        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId, cancellationToken);
        
        if (notification == null || notification.WorkspaceId != _workspaceContext.WorkspaceId)
        {
            return Result.Failure(Error.NullValue); // or NotFound error if you have one
        }

        notification.MarkAsRead();

        return Result.Success();
    }
}
