using NexusMail.Application.Abstractions.Messaging;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Identity.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : ICommand<Result>;
