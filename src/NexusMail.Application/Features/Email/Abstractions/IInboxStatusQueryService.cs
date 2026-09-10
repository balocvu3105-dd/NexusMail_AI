using System.Threading;
using System.Threading.Tasks;
using NexusMail.Application.Features.Email.Queries.GetInboxStatus;

namespace NexusMail.Application.Features.Email.Abstractions;

public interface IInboxStatusQueryService
{
    Task<InboxStatusDto> GetStatusAsync(System.Guid workspaceId, CancellationToken cancellationToken = default);
}
