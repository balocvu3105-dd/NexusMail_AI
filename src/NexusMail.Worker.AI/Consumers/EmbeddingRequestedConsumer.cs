using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using NexusMail.Contracts.AI;
using NexusMail.Application.Features.AI.Commands.GenerateEmbedding;

namespace NexusMail.Worker.AI.Consumers;

public sealed class EmbeddingRequestedConsumer : IConsumer<EmbeddingRequestedMessage>
{
    private readonly ISender _sender;
    private readonly ILogger<EmbeddingRequestedConsumer> _logger;

    public EmbeddingRequestedConsumer(ISender sender, ILogger<EmbeddingRequestedConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EmbeddingRequestedMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing Embedding request for EmailId: {EmailId}", message.EmailId);

        var result = await _sender.Send(new GenerateEmbeddingCommand(message.EmailId, message.WorkspaceId));

        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully generated embedding for EmailId: {EmailId}", message.EmailId);
        }
        else
        {
            _logger.LogWarning("Permanent failure generating embedding for EmailId: {EmailId}. Error: {Error}", message.EmailId, result.Error);
        }
    }
}
