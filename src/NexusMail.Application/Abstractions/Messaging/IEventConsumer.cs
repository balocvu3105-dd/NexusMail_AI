using System.Threading.Tasks;

namespace NexusMail.Application.Abstractions.Messaging;

public interface IEventConsumer<in TMessage> where TMessage : class
{
    Task ConsumeAsync(TMessage message);
}
