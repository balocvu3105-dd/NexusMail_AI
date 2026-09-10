using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using NexusMail.Automation.RuleEngine;
using NexusMail.Contracts.Automation;

namespace NexusMail.Worker.Automation.Consumers;

public sealed class EvaluateRulesConsumer : IConsumer<EvaluateRulesMessage>
{
    private readonly IRuleEvaluationService _ruleEvaluationService;
    private readonly ILogger<EvaluateRulesConsumer> _logger;

    public EvaluateRulesConsumer(
        IRuleEvaluationService ruleEvaluationService,
        ILogger<EvaluateRulesConsumer> logger)
    {
        _ruleEvaluationService = ruleEvaluationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EvaluateRulesMessage> context)
    {
        _logger.LogInformation("Evaluating immediate rules for Email {EmailId}", context.Message.EmailId);
        await _ruleEvaluationService.EvaluateAsync(context.Message, context.CancellationToken);
    }
}
