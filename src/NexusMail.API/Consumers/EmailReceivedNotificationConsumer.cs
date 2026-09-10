using System.Text.Json;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NexusMail.API.Hubs;
using NexusMail.Application.Abstractions.Persistence;
using NexusMail.Application.Features.Notification.DTOs;
using NexusMail.Contracts.Email;
using NexusMail.Notification.Domain;

namespace NexusMail.API.Consumers;

public class EmailReceivedNotificationConsumer : IConsumer<EmailReceivedMessage>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<EmailReceivedNotificationConsumer> _logger;

    public EmailReceivedNotificationConsumer(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        IHubContext<NotificationHub> hubContext,
        ILogger<EmailReceivedNotificationConsumer> logger)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EmailReceivedMessage> context)
    {
        var message = context.Message;
        
        var sourceEventId = message.CorrelationId;
        if (string.IsNullOrWhiteSpace(sourceEventId))
        {
            sourceEventId = context.MessageId?.ToString() ?? System.Guid.NewGuid().ToString();
        }

        var payload = JsonSerializer.Serialize(new
        {
            emailId = message.EmailId,
            sender = message.Sender,
            hasAttachments = message.HasAttachments
        });

        var notificationItem = NotificationItem.Create(
            workspaceId: message.WorkspaceId,
            type: NotificationType.EmailReceived,
            sourceEventId: sourceEventId,
            title: $"New Email from {message.SenderName}",
            message: message.Subject,
            payloadJson: payload,
            actionUrl: $"/emails/{message.EmailId}"
        );

        try
        {
            await _notificationRepository.AddAsync(notificationItem, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            _logger.LogInformation(ex, "Duplicate notification detected for SourceEventId {SourceEventId}. Skipping.", sourceEventId);
            return;
        }

        var dto = new NotificationDto
        {
            Id = notificationItem.Id,
            Type = notificationItem.Type.ToString(),
            Title = notificationItem.Title,
            Message = notificationItem.Message,
            ActionUrl = notificationItem.ActionUrl,
            Payload = JsonDocument.Parse(notificationItem.PayloadJson),
            IsRead = notificationItem.IsRead,
            CreatedAt = notificationItem.CreatedAt
        };

        var groupName = $"Workspace-{message.WorkspaceId}";
        await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", dto, context.CancellationToken);
        
        _logger.LogInformation("Notification broadcasted for email {EmailId} to workspace {WorkspaceId}", message.EmailId, message.WorkspaceId);
    }
}
