using NexusMail.Application.Abstractions.Messaging;
using NexusMail.Shared.Common;
using System;

namespace NexusMail.Application.Features.Identity.Commands.Register;

public sealed record RegisterCommand(string Email, string FirstName, string LastName, string Password) : ICommand<Result<Guid>>;
