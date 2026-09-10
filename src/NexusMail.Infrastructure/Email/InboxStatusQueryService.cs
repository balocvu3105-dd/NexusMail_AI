using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Application.Features.Email.Queries.GetInboxStatus;
using NexusMail.Infrastructure.Persistence;

namespace NexusMail.Infrastructure.Email;

internal sealed class InboxStatusQueryService : IInboxStatusQueryService
{
    private readonly ApplicationDbContext _dbContext;

    public InboxStatusQueryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InboxStatusDto> GetStatusAsync(System.Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var baseQuery = from email in _dbContext.Emails
                        join account in _dbContext.EmailAccounts on email.AccountId equals account.Id
                        where account.WorkspaceId == workspaceId
                        select email;

        var totalEmails = await baseQuery.CountAsync(cancellationToken);
        
        var summaryProcessed = await (from email in baseQuery
                                      join ai in _dbContext.AIAnalyses on email.Id equals ai.EmailId
                                      where ai.Summary != null && ai.Summary != ""
                                      select email).CountAsync(cancellationToken);
            
        var priorityProcessed = await (from email in baseQuery
                                       join ai in _dbContext.AIAnalyses on email.Id equals ai.EmailId
                                       where ai.Priority != null && ai.Priority != ""
                                       select email).CountAsync(cancellationToken);
            
        var classificationProcessed = await (from email in baseQuery
                                             join ai in _dbContext.AIAnalyses on email.Id equals ai.EmailId
                                             where ai.Category != null && ai.Category != ""
                                             select email).CountAsync(cancellationToken);

        var isReady = totalEmails > 0 && 
                      summaryProcessed >= totalEmails && 
                      priorityProcessed >= totalEmails && 
                      classificationProcessed >= totalEmails;

        return new InboxStatusDto(
            totalEmails,
            summaryProcessed,
            priorityProcessed,
            classificationProcessed,
            isReady
        );
    }
}
