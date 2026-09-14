using MassTransit;
using System.Threading.Tasks;

namespace NexusMail.Worker.AI.Consumers
{
    public class SummaryRequestedConsumer : IConsumer<NexusMail.Contracts.AI.SummaryRequestedMessage> { public Task Consume(ConsumeContext<NexusMail.Contracts.AI.SummaryRequestedMessage> context) => Task.CompletedTask; }
    public class PriorityRequestedConsumer : IConsumer<NexusMail.Contracts.AI.SummaryRequestedMessage> { public Task Consume(ConsumeContext<NexusMail.Contracts.AI.SummaryRequestedMessage> context) => Task.CompletedTask; }
    public class ClassificationRequestedConsumer : IConsumer<NexusMail.Contracts.AI.SummaryRequestedMessage> { public Task Consume(ConsumeContext<NexusMail.Contracts.AI.SummaryRequestedMessage> context) => Task.CompletedTask; }
    public class EmbeddingRequestedConsumer : IConsumer<NexusMail.Contracts.AI.SummaryRequestedMessage> { public Task Consume(ConsumeContext<NexusMail.Contracts.AI.SummaryRequestedMessage> context) => Task.CompletedTask; }
}

namespace NexusMail.Contracts.AI
{
    public class SummaryRequestedMessage { }
}
