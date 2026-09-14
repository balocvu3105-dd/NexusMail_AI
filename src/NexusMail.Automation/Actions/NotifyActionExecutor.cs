using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Contracts.Automation;
using NexusMail.Domain.Automation.Enums;
using MassTransit;

namespace NexusMail.Automation.Actions;

public sealed class NotifyActionExecutor : IActionExecutor
{
    private readonly IPublishEndpoint _publishEndpoint;

    public ActionType ActionType => ActionType.SendNotification;

    public NotifyActionExecutor(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task<ActionResult> ExecuteAsync(string idempotencyKey, string parametersJson, EvaluateRulesMessage context, CancellationToken cancellationToken = default)
    {
        string message = "";
        if (!string.IsNullOrWhiteSpace(parametersJson) && parametersJson != "[]" && parametersJson != "{}")
        {
            try
            {
                var doc = JsonDocument.Parse(parametersJson);
                if (doc.RootElement.TryGetProperty("message", out var prop))
                {
                    message = prop.GetString() ?? "";
                }
            }
            catch
            {
                return ActionResult.PermanentFailure;
            }
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return ActionResult.PermanentFailure;
        }

        // Ideally publish SendPushNotificationCommand
        return ActionResult.Success;
    }
}
