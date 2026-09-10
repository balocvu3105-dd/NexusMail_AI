using System;
using MediatR;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.AI.Commands.GenerateReply;

public record GenerateReplyCommand(Guid EmailId, Guid WorkspaceId) : IRequest<Result<Guid>>;
