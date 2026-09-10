using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Email.Queries.GetInbox;

public sealed class GetInboxQueryHandler : IRequestHandler<GetInboxQuery, Result<List<InboxMessageDto>>>
{
    private readonly IInboxQueryService _inboxQueryService;

    public GetInboxQueryHandler(IInboxQueryService inboxQueryService)
    {
        _inboxQueryService = inboxQueryService;
    }

    public async Task<Result<List<InboxMessageDto>>> Handle(GetInboxQuery request, CancellationToken cancellationToken)
    {
        var messages = await _inboxQueryService.GetInboxAsync(request.WorkspaceId, cancellationToken);
        return Result.Success(messages);
    }
}

