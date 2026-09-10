using System;
using NexusMail.Application.Abstractions.Messaging;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Email.Commands.UpdateEmailAccountSettings;

public record UpdateEmailAccountSettingsCommand(Guid Id, bool SyncEnabled) : ICommand<Result>;
