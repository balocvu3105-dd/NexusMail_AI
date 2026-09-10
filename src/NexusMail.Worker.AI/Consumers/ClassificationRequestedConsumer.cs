using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using NexusMail.Contracts.AI;
using NexusMail.Application.Features.AI.Commands.CategorizeEmail;

namespace NexusMail.Worker.AI.Consumers;

public sealed class ClassificationRequestedConsumer : IConsumer<ClassificationRequestedMessage>
{
    private readonly ISender _sender;
    private readonly ILogger<ClassificationRequestedConsumer> _logger;

    public ClassificationRequestedConsumer(ISender sender, ILogger<ClassificationRequestedConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ClassificationRequestedMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing Classification request for EmailId: {EmailId}", message.EmailId);

        var result = await _sender.Send(new CategorizeEmailCommand(message.EmailId, message.WorkspaceId));

        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully classified EmailId: {EmailId}", message.EmailId);
        }
        else
        {
            _logger.LogWarning("Permanent failure classifying EmailId: {EmailId}. Error: {Error}", message.EmailId, result.Error);
        }
    }
}
