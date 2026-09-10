using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using NexusMail.Contracts.AI;
using NexusMail.Application.Features.AI.Commands.ScorePriority;

namespace NexusMail.Worker.AI.Consumers;

public sealed class PriorityRequestedConsumer : IConsumer<PriorityRequestedMessage>
{
    private readonly ISender _sender;
    private readonly ILogger<PriorityRequestedConsumer> _logger;

    public PriorityRequestedConsumer(ISender sender, ILogger<PriorityRequestedConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PriorityRequestedMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing Priority request for EmailId: {EmailId}", message.EmailId);

        var result = await _sender.Send(new ScorePriorityCommand(message.EmailId, message.WorkspaceId));

        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully assessed priority for EmailId: {EmailId}", message.EmailId);
        }
        else
        {
            _logger.LogWarning("Permanent failure assessing priority for EmailId: {EmailId}. Error: {Error}", message.EmailId, result.Error);
        }
    }
}
