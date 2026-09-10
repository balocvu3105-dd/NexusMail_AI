using System;
using System.Threading;
using System.Threading.Tasks;

namespace NexusMail.Application.Features.Email.Abstractions;

public interface IEmailSynchronizationService
{
    Task SyncEmailAccountAsync(Guid emailAccountId, string correlationId, CancellationToken cancellationToken = default);
}
