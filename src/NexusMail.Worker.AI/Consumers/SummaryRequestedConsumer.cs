using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using NexusMail.Contracts.AI;
using NexusMail.Application.Features.AI.Commands.GenerateSummary;

namespace NexusMail.Worker.AI.Consumers;

public sealed class SummaryRequestedConsumer : IConsumer<SummaryRequestedMessage>
{
    private readonly ISender _sender;
    private readonly ILogger<SummaryRequestedConsumer> _logger;

    public SummaryRequestedConsumer(ISender sender, ILogger<SummaryRequestedConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SummaryRequestedMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing Summary request for EmailId: {EmailId}", message.EmailId);

        // Note: Transient failures will throw AIProviderTransientException from the Handler and bubble up to here,
        // which will trigger MassTransit's configured UseMessageRetry for Transient Exceptions.
        var result = await _sender.Send(new GenerateSummaryCommand(message.EmailId, message.WorkspaceId));

        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully generated summary for EmailId: {EmailId}", message.EmailId);
        }
        else
        {
            _logger.LogWarning("Permanent failure generating summary for EmailId: {EmailId}. Error: {Error}", message.EmailId, result.Error);
            // We DO NOT throw here. The Handler has already persisted the state as 'Failed'.
            // Returning normally will ACK the message, avoiding useless retries for permanent failures.
        }
    }
}
