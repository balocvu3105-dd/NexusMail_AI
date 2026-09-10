using System.Collections.Generic;
using NexusMail.Application.Abstractions.Messaging;
using NexusMail.Application.Features.Notification.DTOs;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.Notification.Queries;

public sealed record GetNotificationsQuery(
    bool UnreadOnly = false,
    int Limit = 20) : IQuery<Result<IReadOnlyList<NotificationDto>>>;
