using NexusMail.Application.Abstractions.Messaging;
using NexusMail.Application.Features.Identity.Results;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Identity.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<Result<AuthenticationResult>>;
