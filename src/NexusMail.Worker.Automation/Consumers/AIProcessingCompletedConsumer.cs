using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NexusMail.Automation.RuleEngine;
using NexusMail.Contracts.AI;
using NexusMail.Contracts.Automation;
using NexusMail.Infrastructure.Persistence;

namespace NexusMail.Worker.Automation.Consumers;

public sealed class AIProcessingCompletedConsumer : IConsumer<AIProcessingCompletedMessage>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRuleEvaluationService _ruleEvaluationService;
    private readonly ILogger<AIProcessingCompletedConsumer> _logger;

    public AIProcessingCompletedConsumer(
        ApplicationDbContext dbContext,
        IRuleEvaluationService ruleEvaluationService,
        ILogger<AIProcessingCompletedConsumer> logger)
    {
        _dbContext = dbContext;
        _ruleEvaluationService = ruleEvaluationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AIProcessingCompletedMessage> context)
    {
        var msg = context.Message;
        
        var email = await _dbContext.Emails
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == msg.EmailId, context.CancellationToken);

        if (email == null)
        {
            _logger.LogWarning("Email {EmailId} not found. Skipping automation rules.", msg.EmailId);
            return;
        }

        // Only add AI metadata keys for capabilities that Succeeded.
        // Absent key → GetAIProperty throws MissingContextException → RuleEvaluator returns NotApplicable.
        // A key present with null value would instead return null (wrong — "unknown" must not become "false").
        // Keys are normalized to lowercase to match ConditionCompiler.ToLowerInvariant() property lookup.
        var aiMetadata = new Dictionary<string, object>();

        if (msg.SummaryResult?.Succeeded == true && msg.SummaryResult.Value != null)
            aiMetadata["summary"] = msg.SummaryResult.Value;

        if (msg.PriorityResult?.Succeeded == true && msg.PriorityResult.Value.HasValue)
            aiMetadata["priorityscore"] = msg.PriorityResult.Value.Value;

        if (msg.ClassificationResult?.Succeeded == true && msg.ClassificationResult.Value != null)
        {
            var cls = msg.ClassificationResult.Value;
            if (cls.Category != null)
                aiMetadata["category"] = cls.Category;
            if (cls.Language != null)
                aiMetadata["language"] = cls.Language;
            if (cls.Tags is { Count: > 0 })
                aiMetadata["tags"] = cls.Tags;
        }

        var evaluateMsg = new EvaluateRulesMessage
        {
            EmailId = msg.EmailId,
            WorkspaceId = msg.WorkspaceId,
            EmailAccountId = email.AccountId,
            Subject = email.Subject,
            Sender = email.Sender,
            SenderDomain = email.Sender.Split('@').LastOrDefault() ?? "",
            HasAttachments = false,
            ReceivedAt = email.ReceivedAt,
            AIMetadata = aiMetadata
        };

        _logger.LogInformation("Evaluating post-AI rules for Email {EmailId}", msg.EmailId);
        await _ruleEvaluationService.EvaluateAsync(evaluateMsg, context.CancellationToken);
    }
}
