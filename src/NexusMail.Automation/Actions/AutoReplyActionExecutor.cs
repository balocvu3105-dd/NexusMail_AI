using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Contracts.Automation;
using NexusMail.Domain.Automation.Enums;
using MassTransit;

namespace NexusMail.Automation.Actions;

public sealed class AutoReplyActionExecutor : IActionExecutor
{
    private readonly IPublishEndpoint _publishEndpoint;

    public ActionType ActionType => ActionType.AutoReply;

    public AutoReplyActionExecutor(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task<ActionResult> ExecuteAsync(string idempotencyKey, string parametersJson, EvaluateRulesMessage context, CancellationToken cancellationToken = default)
    {
        // For Draft Generation, we just publish an event for the AI Worker to handle it.
        // We shouldn't do long-running LLM calls directly in the Automation Rule engine execution.
        
        string promptTemplate = "";
        if (!string.IsNullOrWhiteSpace(parametersJson) && parametersJson != "[]" && parametersJson != "{}")
        {
            try
            {
                var doc = JsonDocument.Parse(parametersJson);
                if (doc.RootElement.TryGetProperty("promptTemplate", out var prop))
                {
                    promptTemplate = prop.GetString() ?? "";
                }
            }
            catch
            {
                // Ignore parse errors, use default prompt
            }
        }

        var cmd = new DraftGenerationRequestedEvent
        {
            WorkspaceId = context.WorkspaceId,
            EmailId = context.EmailId,
            PromptTemplate = promptTemplate
        };

        await _publishEndpoint.Publish(cmd, cancellationToken);

        return ActionResult.Success;
    }
}


