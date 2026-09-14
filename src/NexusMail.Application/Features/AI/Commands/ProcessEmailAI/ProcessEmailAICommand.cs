using System;
using MediatR;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.AI.Commands.ProcessEmailAI;

public sealed record ProcessEmailAICommand(Guid EmailId, Guid WorkspaceId) : IRequest<Result<Guid>>;
