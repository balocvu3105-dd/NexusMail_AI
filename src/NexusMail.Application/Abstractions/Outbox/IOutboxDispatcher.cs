using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NexusMail.Application.Abstractions.Outbox;

public interface IOutboxDispatcher
{
    Task DispatchUnprocessedMessagesAsync(CancellationToken cancellationToken = default);
}
