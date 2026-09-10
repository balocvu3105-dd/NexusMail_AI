using System;
using System.Threading;
using System.Threading.Tasks;

namespace NexusMail.Application.Abstractions.Outbox;

public interface IOutboxStore
{
    Task InsertAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}
