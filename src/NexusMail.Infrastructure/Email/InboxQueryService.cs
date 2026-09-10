using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Application.Features.Email.Queries.GetInbox;
using NexusMail.Infrastructure.Persistence;

namespace NexusMail.Infrastructure.Email;

internal sealed class InboxQueryService : IInboxQueryService
{
    private readonly ApplicationDbContext _dbContext;

    public InboxQueryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<InboxMessageDto>> GetInboxAsync(System.Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var query = from email in _dbContext.Emails
                    join account in _dbContext.EmailAccounts on email.AccountId equals account.Id
                    where account.WorkspaceId == workspaceId
                    join ai in _dbContext.AIAnalyses on email.Id equals ai.EmailId into aiGroup
                    from ai in aiGroup.DefaultIfEmpty()
                    orderby ai.PriorityScore descending, email.ReceivedAt descending
                    select new InboxMessageDto(
                        email.Id,
                        email.MessageId,
                        email.Sender,
                        email.Subject,
                        email.Content,
                        email.ReceivedAt,
                        ai != null ? ai.Summary : "",
                        ai != null ? ai.PriorityScore : 0,
                        ai != null ? ai.Priority : "Normal",
                        ai != null ? ai.Category : "Unknown"
                    );

        return await query.Take(50).ToListAsync(cancellationToken);
    }
}
