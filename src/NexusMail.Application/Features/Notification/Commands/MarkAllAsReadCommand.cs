using NexusMail.Application.Abstractions.Messaging;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.Notification.Commands;

public sealed record MarkAllAsReadCommand() : ICommand<Result>;
