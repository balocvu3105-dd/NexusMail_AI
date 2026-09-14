using System.Threading.Tasks;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Features.AI.Commands.ProcessEmailAI;
using NexusMail.Contracts.AI;

namespace NexusMail.Worker.AI.Consumers;

public sealed class AIProcessingRequestedConsumer : IConsumer<AIProcessingRequestedMessage>
{
    private readonly ISender _sender;
    private readonly ILogger<AIProcessingRequestedConsumer> _logger;

    public AIProcessingRequestedConsumer(ISender sender, ILogger<AIProcessingRequestedConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AIProcessingRequestedMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Received AIProcessingRequestedMessage for EmailId: {EmailId}", msg.EmailId);

        var command = new ProcessEmailAICommand(msg.EmailId, msg.WorkspaceId);
        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsFailure)
        {
            _logger.LogError("Failed to process AI for EmailId: {EmailId}. Error: {Error}", msg.EmailId, result.Error);
            throw new System.Exception($"AI Processing Failed: {result.Error.Code} - {result.Error.Description}");
        }

        _logger.LogInformation("Successfully completed AI processing for EmailId: {EmailId}", msg.EmailId);
    }
}
