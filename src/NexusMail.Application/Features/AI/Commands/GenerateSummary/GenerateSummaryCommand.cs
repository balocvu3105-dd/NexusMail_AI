using System;
using MediatR;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.AI.Commands.GenerateSummary;

public record GenerateSummaryCommand(Guid EmailId, Guid WorkspaceId) : IRequest<Result<Guid>>;
