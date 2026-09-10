using System.Threading;
using System.Threading.Tasks;

namespace NexusMail.Application.Abstractions.Messaging;

public interface IMessageBus
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
}
