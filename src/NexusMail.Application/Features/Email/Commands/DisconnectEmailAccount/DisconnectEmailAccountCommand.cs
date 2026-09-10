using System;
using NexusMail.Application.Abstractions.Messaging;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Email.Commands.DisconnectEmailAccount;

public record DisconnectEmailAccountCommand(Guid Id) : ICommand<Result>;
