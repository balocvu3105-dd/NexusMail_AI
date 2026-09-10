using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Abstractions.Outbox;
using NexusMail.Automation.RuleEngine;
using NexusMail.Contracts.Automation;
using NexusMail.Domain.Automation.Entities;
using NexusMail.Infrastructure.Persistence;
using NexusMail.Application.Abstractions.Context;
using System.Text.Json;

namespace NexusMail.Worker.Automation.Services;

public sealed class RuleEvaluationService : IRuleEvaluationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRuleEvaluator _ruleEvaluator;
    private readonly ILogger<RuleEvaluationService> _logger;
    private readonly IExecutionContextAccessor _executionContextAccessor;

    public RuleEvaluationService(
        ApplicationDbContext dbContext,
        IRuleEvaluator ruleEvaluator,
        ILogger<RuleEvaluationService> logger,
        IExecutionContextAccessor executionContextAccessor)
    {
        _dbContext = dbContext;
        _ruleEvaluator = ruleEvaluator;
        _logger = logger;
        _executionContextAccessor = executionContextAccessor;
    }

    public async Task EvaluateAsync(EvaluateRulesMessage message, CancellationToken cancellationToken)
    {
        var rules = await _dbContext.AutomationRules
            .Where(r => r.WorkspaceId == message.WorkspaceId && r.IsEnabled)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var rule in rules)
        {
            var sw = Stopwatch.StartNew();
            
            RuleEvaluationResult result = RuleEvaluationResult.NoMatch;
            string? errorMessage = null;

            try
            {
                result = _ruleEvaluator.Evaluate(rule.ConditionsJson, message);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                result = RuleEvaluationResult.NoMatch;
                _logger.LogError(ex, "Error evaluating rule {RuleId}", rule.Id);
            }

            sw.Stop();
            var elapsed = sw.Elapsed.TotalMilliseconds;

            string resultStr = result switch
            {
                RuleEvaluationResult.Match => "Matched",
                RuleEvaluationResult.NoMatch => "Evaluated",
                RuleEvaluationResult.NotApplicable => "NotApplicable",
                _ => "Evaluated"
            };

            if (errorMessage != null) resultStr = "PermanentFailure";

            if (result == RuleEvaluationResult.Match && !rule.DryRun)
            {
                // Execute Match Flow in its own transaction to enforce idempotency reliably
                await ProcessMatchAsync(rule, message, resultStr, elapsed, errorMessage, cancellationToken);
            }
            else
            {
                // For NoMatch or NotApplicable, just record metric. No execution needed.
                var metric = AutomationRuleMetric.Create(rule.Id, rule.RuleVersion, message.EmailId, resultStr, elapsed, errorMessage);
                _dbContext.AutomationRuleMetrics.Add(metric);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private async Task ProcessMatchAsync(AutomationRule rule, EvaluateRulesMessage message, string resultStr, double elapsed, string? errorMessage, CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var execution = AutomationExecution.Create(rule.Id, message.EmailId, "Matched");
            _dbContext.AutomationExecutions.Add(execution);

            var metric = AutomationRuleMetric.Create(rule.Id, rule.RuleVersion, message.EmailId, resultStr, elapsed, errorMessage);
            _dbContext.AutomationRuleMetrics.Add(metric);

            var actionEvent = new NexusMail.Application.Features.Automation.Events.RuleMatchedDomainEvent
            {
                ExecutionId = execution.Id,
                RuleId = rule.Id,
                EmailId = message.EmailId,
                WorkspaceId = message.WorkspaceId,
                ActionsJson = rule.ActionsJson,
                EvaluateContext = message
            };

            execution.RaiseRuleMatchedEvent(actionEvent);

            // Create Pending AutomationActionExecutions based on ActionsJson
            try
            {
                var actions = JsonSerializer.Deserialize<ActionDefinitionDto[]>(rule.ActionsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (actions != null)
                {
                    for (int i = 0; i < actions.Length; i++)
                    {
                        var actionExecution = AutomationActionExecution.Create(execution.Id, i.ToString(), actions[i].Type);
                        _dbContext.AutomationActionExecutions.Add(actionExecution);
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse ActionsJson for Rule {RuleId}", rule.Id);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Successfully processed rule {RuleId} match for email {EmailId}", rule.Id, message.EmailId);
        }
        catch (DbUpdateException ex)
        {
            // Unique constraint violation (Race condition: rule evaluated concurrently)
            await transaction.RollbackAsync(cancellationToken);
            
            // Note: Clear change tracker for these entities so subsequent rules in the loop can proceed
            _dbContext.ChangeTracker.Clear();

            _logger.LogWarning(ex, "Concurrent evaluation detected for Rule {RuleId} and Email {EmailId}. Duplicate execution skipped.", rule.Id, message.EmailId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _dbContext.ChangeTracker.Clear();
            _logger.LogError(ex, "Failed to process rule {RuleId} match for email {EmailId}", rule.Id, message.EmailId);
            throw;
        }
    }
}

public class ActionDefinitionDto
{
    public string Type { get; set; } = string.Empty;
    public object? Parameters { get; set; }
}
