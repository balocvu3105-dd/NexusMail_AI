using System.Threading;
using System.Threading.Tasks;

namespace NexusMail.Application.Abstractions.Outbox;

/// <summary>
/// Inbox Pattern abstraction for handling incoming webhooks and events safely.
/// </summary>
public interface IIncomingMessageStore
{
    Task StoreMessageAsync(string messageId, string type, string content, CancellationToken cancellationToken = default);
    Task<bool> HasMessageBeenProcessedAsync(string messageId, CancellationToken cancellationToken = default);
    Task MarkMessageProcessedAsync(string messageId, CancellationToken cancellationToken = default);
}
