using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Application.Features.Email.Queries.GetInbox;

namespace NexusMail.Application.Features.Email.Abstractions;

public interface IInboxQueryService
{
    Task<List<InboxMessageDto>> GetInboxAsync(System.Guid workspaceId, CancellationToken cancellationToken = default);
}
