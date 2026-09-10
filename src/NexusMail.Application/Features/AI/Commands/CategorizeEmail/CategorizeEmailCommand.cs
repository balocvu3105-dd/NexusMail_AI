using System;
using MediatR;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.AI.Commands.CategorizeEmail;

public record CategorizeEmailCommand(Guid EmailId, Guid WorkspaceId) : IRequest<Result<Guid>>;
