using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Notification.Domain;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.Notification.Commands;

public sealed class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, Result>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IWorkspaceContext _workspaceContext;

    public MarkAllAsReadCommandHandler(
        INotificationRepository notificationRepository, 
        IWorkspaceContext workspaceContext)
    {
        _notificationRepository = notificationRepository;
        _workspaceContext = workspaceContext;
    }

    public async Task<Result> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        if (_workspaceContext.WorkspaceId == null)
        {
            return Result.Failure(Error.NullValue);
        }

        await _notificationRepository.MarkAllAsReadAsync(_workspaceContext.WorkspaceId.Value, cancellationToken);
        
        return Result.Success();
    }
}
