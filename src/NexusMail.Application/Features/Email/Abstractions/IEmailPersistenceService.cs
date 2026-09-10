using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NexusMail.Application.Features.Email.Abstractions;

public interface IEmailPersistenceService
{
    Task SaveEmailsAsync(IReadOnlyList<object> domainMessages, CancellationToken cancellationToken = default);
    Task<HashSet<string>> GetExistingMessageIdsAsync(System.Guid emailAccountId, IEnumerable<string> messageIds, CancellationToken cancellationToken = default);
}
