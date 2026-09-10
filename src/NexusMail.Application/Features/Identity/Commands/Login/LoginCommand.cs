using NexusMail.Application.Abstractions.Messaging;
using NexusMail.Application.Features.Identity.Results;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Identity.Commands.Login;

public sealed record LoginCommand(string Email, string Password, string? DeviceName, string? IpAddress, string? UserAgent) : ICommand<Result<AuthenticationResult>>;
