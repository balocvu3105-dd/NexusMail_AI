using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Email.Queries.GetInboxStatus;

public sealed class GetInboxStatusQueryHandler : IRequestHandler<GetInboxStatusQuery, Result<InboxStatusDto>>
{
    private readonly IInboxStatusQueryService _statusQueryService;

    public GetInboxStatusQueryHandler(IInboxStatusQueryService statusQueryService)
    {
        _statusQueryService = statusQueryService;
    }

    public async Task<Result<InboxStatusDto>> Handle(GetInboxStatusQuery request, CancellationToken cancellationToken)
    {
        var status = await _statusQueryService.GetStatusAsync(request.WorkspaceId, cancellationToken);
        return Result.Success(status);
    }
}
