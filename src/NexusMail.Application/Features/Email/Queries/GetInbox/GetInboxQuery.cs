using System.Collections.Generic;
using MediatR;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Email.Queries.GetInbox;

public record GetInboxQuery(System.Guid WorkspaceId) : IRequest<Result<List<InboxMessageDto>>>;
