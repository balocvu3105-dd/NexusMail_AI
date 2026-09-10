using MediatR;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Email.Queries.GetInboxStatus;

public record InboxStatusDto(
    int TotalEmails,
    int SummaryProcessed,
    int PriorityProcessed,
    int ClassificationProcessed,
    bool IsReady
);

public record GetInboxStatusQuery(System.Guid WorkspaceId) : IRequest<Result<InboxStatusDto>>;
