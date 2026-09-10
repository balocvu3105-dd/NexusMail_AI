using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Domain.Email.Entities;

namespace NexusMail.Application.Features.Email.Abstractions;

public interface IEmailAccountRepository
{
    Task<EmailAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<EmailAccount> Items, int TotalCount)> GetPagedByWorkspaceIdAsync(Guid workspaceId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(EmailAccount emailAccount, CancellationToken cancellationToken = default);
    Task UpdateAsync(EmailAccount emailAccount, CancellationToken cancellationToken = default);
    Task DeleteAsync(EmailAccount emailAccount, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid workspaceId, string emailAddress, CancellationToken cancellationToken = default);
    Task<List<EmailAccount>> GetAccountsDueForSyncAsync(int batchSize, CancellationToken cancellationToken = default);
}
