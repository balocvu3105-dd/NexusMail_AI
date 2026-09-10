using System;
using System.Text.Json;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NexusMail.Automation.Actions;
using NexusMail.Contracts.Automation;
using NexusMail.Domain.Automation.Enums;
using NexusMail.Infrastructure.Persistence;
using System.Linq;

namespace NexusMail.Worker.Automation.Consumers;

public sealed class ActionExecutionConsumer : IConsumer<ActionExecutionEvent>
{
    private readonly ActionExecutorFactory _executorFactory;
    private readonly ILogger<ActionExecutionConsumer> _logger;
    private readonly ApplicationDbContext _dbContext;

    public ActionExecutionConsumer(
        ActionExecutorFactory executorFactory,
        ILogger<ActionExecutionConsumer> logger,
        ApplicationDbContext dbContext)
    {
        _executorFactory = executorFactory;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<ActionExecutionEvent> context)
    {
        var msg = context.Message;
        
        try
        {
            var actions = JsonSerializer.Deserialize<ActionDefinition[]>(msg.ActionsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (actions == null || actions.Length == 0) return;

            for (int i = 0; i < actions.Length; i++)
            {
                var action = actions[i];
                var actionKey = i.ToString();

                // Idempotency check
                var actionExecution = await _dbContext.AutomationActionExecutions
                    .FirstOrDefaultAsync(x => x.ExecutionId == msg.ExecutionId && x.ActionKey == actionKey, context.CancellationToken);

                if (actionExecution == null)
                {
                    _logger.LogWarning("Action execution record not found for ExecutionId {ExecutionId} and ActionKey {ActionKey}. Skipping.", msg.ExecutionId, actionKey);
                    continue;
                }

                if (actionExecution.Status == "Success")
                {
                    _logger.LogInformation("Action {ActionKey} for ExecutionId {ExecutionId} already executed successfully. Skipping duplicate execution.", actionKey, msg.ExecutionId);
                    continue;
                }

                if (actionExecution.Status == "Unknown")
                {
                    _logger.LogWarning("Action {ActionKey} for ExecutionId {ExecutionId} is in Unknown state. Manual intervention required. Skipping.", actionKey, msg.ExecutionId);
                    continue;
                }

                if (actionExecution.Status == "Executing")
                {
                    // CRASH RECOVERY LOGIC:
                    // If it is 'Executing', a previous attempt crashed. We cannot blindly retry
                    // because the external action might have completed.
                    _logger.LogError("Action {ActionKey} for ExecutionId {ExecutionId} was found in Executing state upon redelivery. Outcome is unknown. Marking as Unknown to prevent double execution.", actionKey, msg.ExecutionId);
                    actionExecution.MarkUnknown("Process crashed during execution. Outcome unknown.");
                    
                    var auditCrash = NexusMail.Domain.Automation.Entities.AutomationAudit.Create(
                        msg.RuleId, msg.EmailId, msg.ExecutionId, action.Type, "Unknown", "Process crashed during execution. Outcome unknown."
                    );
                    _dbContext.AutomationAudits.Add(auditCrash);
                    
                    // We save immediately and skip to not block other actions in this event
                    await _dbContext.SaveChangesAsync(context.CancellationToken);
                    continue;
                }

                // Transition to Executing before making any external calls
                actionExecution.MarkExecuting();
                await _dbContext.SaveChangesAsync(context.CancellationToken);

                if (Enum.TryParse<ActionType>(action.Type, out var actionType))
                {
                    var executor = _executorFactory.GetExecutor(actionType);
                    if (executor != null)
                    {
                        string parametersJson = action.Parameters != null 
                            ? JsonSerializer.Serialize(action.Parameters) 
                            : "{}";
                            
                        var result = await executor.ExecuteAsync(parametersJson, msg.EvaluateContext, context.CancellationToken);
                        
                        _logger.LogInformation("Action {ActionType} executed for Email {EmailId} with result {Result}", actionType, msg.EmailId, result);

                        var audit = NexusMail.Domain.Automation.Entities.AutomationAudit.Create(
                            msg.RuleId, 
                            msg.EmailId, 
                            msg.ExecutionId,
                            actionType.ToString(), 
                            result.ToString()
                        );
                        _dbContext.AutomationAudits.Add(audit);

                        if (result == ActionResult.Success)
                        {
                            actionExecution.MarkSuccess();
                        }
                        else if (result == ActionResult.PermanentFailure)
                        {
                            actionExecution.MarkFailed();
                        }
                        else if (result == ActionResult.Unknown)
                        {
                            // Process didn't crash, but the executor reported the outcome as Unknown
                            // (e.g., a network timeout after sending the request to a non-idempotent provider).
                            // We MUST NOT retry.
                            actionExecution.MarkUnknown("Executor reported Unknown outcome (e.g. network timeout).");
                        }
                        else if (result == ActionResult.TransientFailure)
                        {
                            // Transient Failure implies the external action did NOT complete, 
                            // and it is SAFE to retry. We mark it Failed (or Pending for retry).
                            // Let's mark it Failed temporarily, but increment Retry.
                            actionExecution.IncrementRetry();
                            actionExecution.MarkFailed("Transient failure. Will be retried by MassTransit.");
                            
                            // Save the failure state before throwing so we can track the retry count
                            await _dbContext.SaveChangesAsync(context.CancellationToken); 
                            throw new Exception($"Action {actionType} encountered transient failure. Requesting MassTransit retry.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No executor found for ActionType: {ActionType}", actionType);
                        var audit = NexusMail.Domain.Automation.Entities.AutomationAudit.Create(
                            msg.RuleId, msg.EmailId, msg.ExecutionId, actionType.ToString(), "PermanentFailure", "No executor found"
                        );
                        _dbContext.AutomationAudits.Add(audit);
                        actionExecution.MarkFailed("No executor found");
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid ActionType: {ActionTypeString}", action.Type);
                    actionExecution.MarkFailed("Invalid ActionType");
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse ActionsJson for Rule {RuleId}", msg.RuleId);
        }

        // Final save for any successes or permanent failures
        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}

public class ActionDefinition
{
    public string Type { get; set; } = string.Empty;
    public object? Parameters { get; set; }
}
