using System;
using MediatR;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.AI.Commands.ScanForPhishing;

public record ScanForPhishingCommand(Guid EmailId, Guid WorkspaceId) : IRequest<Result<Guid>>;
