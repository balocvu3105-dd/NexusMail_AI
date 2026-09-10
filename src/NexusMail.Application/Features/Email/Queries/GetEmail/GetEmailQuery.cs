using System;
using MediatR;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.Email.Queries.GetEmail;

public record GetEmailQuery(Guid Id) : IRequest<Result<EmailDetailsDto>>;
