using MassTransit;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Abstractions.AI;
using NexusMail.Contracts.AI;
using NexusMail.Domain.AI.Enums;
using NexusMail.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace NexusMail.Worker.AI.Consumers;

public class AIFaultConsumer : IConsumer<Fault<AIProcessingRequestedMessage>>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AIFaultConsumer> _logger;

    public AIFaultConsumer(ApplicationDbContext dbContext, ILogger<AIFaultConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<Fault<AIProcessingRequestedMessage>> context)
    {
        var emailId = context.Message.Message.EmailId;
        _logger.LogWarning("Handling exhausted fault for EmailId: {EmailId}", emailId);

        var analysis = await _dbContext.AIAnalyses.FirstOrDefaultAsync(a => a.EmailId == emailId);
        if (analysis != null && analysis.ProcessingState == AIProcessingStatus.Processing)
        {
            if (analysis.ProcessingAttemptId.HasValue)
            {
                analysis.FailProcessing(analysis.ProcessingAttemptId.Value, "Exhausted all retries. " + context.Message.Exceptions.FirstOrDefault()?.Message);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Marked AIAnalysis for EmailId {EmailId} as Failed due to exhausted retries.", emailId);
            }
        }
    }
}
