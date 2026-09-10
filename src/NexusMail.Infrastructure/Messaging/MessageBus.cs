using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Abstractions.Messaging;

namespace NexusMail.Infrastructure.Messaging;

public sealed class MessageBus : IMessageBus
{
    private readonly ILogger<MessageBus> _logger;

    public MessageBus(ILogger<MessageBus> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        _logger.LogInformation("MessageBus: Published message of type {MessageType}", typeof(T).Name);
        return Task.CompletedTask;
    }
}
