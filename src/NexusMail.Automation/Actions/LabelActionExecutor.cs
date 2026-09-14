using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Contracts.Automation;
using NexusMail.Domain.Automation.Enums;
using MassTransit;

namespace NexusMail.Automation.Actions;

public sealed class LabelActionExecutor : IActionExecutor
{
    private readonly IPublishEndpoint _publishEndpoint;

    public ActionType ActionType => ActionType.ApplyLabel;

    public LabelActionExecutor(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task<ActionResult> ExecuteAsync(string idempotencyKey, string parametersJson, EvaluateRulesMessage context, CancellationToken cancellationToken = default)
    {
        string labelName = "";
        if (!string.IsNullOrWhiteSpace(parametersJson) && parametersJson != "[]" && parametersJson != "{}")
        {
            try
            {
                var doc = JsonDocument.Parse(parametersJson);
                if (doc.RootElement.TryGetProperty("label", out var prop))
                {
                    labelName = prop.GetString() ?? "";
                }
            }
            catch
            {
                return ActionResult.PermanentFailure;
            }
        }

        if (string.IsNullOrWhiteSpace(labelName))
        {
            return ActionResult.PermanentFailure;
        }

        // Ideally, we publish an event that the Email bounded context handles.
        // e.g. ApplyLabelCommand
        // For now, we simulate this event.

        return ActionResult.Success;
    }
}
