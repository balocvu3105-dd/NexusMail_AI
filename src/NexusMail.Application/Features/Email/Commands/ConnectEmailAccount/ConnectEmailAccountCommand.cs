using System;
using NexusMail.Application.Abstractions.Messaging;
using NexusMail.Domain.Email.Enums;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Email.Commands.ConnectEmailAccount;

public record ConnectEmailAccountCommand(
    EmailProvider Provider,
    string AuthCode,
    string RedirectUri) : ICommand<Result<Guid>>;
