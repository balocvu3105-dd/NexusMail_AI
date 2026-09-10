using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Contracts.Automation;
using NexusMail.Domain.Automation.Enums;
using NexusMail.Application.Abstractions.Email;

namespace NexusMail.Automation.Actions;

public sealed class ForwardActionExecutor : IActionExecutor
{
    private readonly IEmailProvider _emailProvider;

    public ActionType ActionType => ActionType.ForwardEmail;

    public ForwardActionExecutor(IEmailProvider emailProvider)
    {
        _emailProvider = emailProvider;
    }

    public async Task<ActionResult> ExecuteAsync(string parametersJson, EvaluateRulesMessage context, CancellationToken cancellationToken = default)
    {
        string forwardTo = "";
        if (!string.IsNullOrWhiteSpace(parametersJson) && parametersJson != "[]" && parametersJson != "{}")
        {
            try
            {
                var doc = JsonDocument.Parse(parametersJson);
                if (doc.RootElement.TryGetProperty("to", out var prop))
                {
                    forwardTo = prop.GetString() ?? "";
                }
            }
            catch
            {
                return ActionResult.PermanentFailure;
            }
        }

        if (string.IsNullOrWhiteSpace(forwardTo))
        {
            return ActionResult.PermanentFailure;
        }

        try
        {
            await _emailProvider.SendEmailAsync(
                to: forwardTo,
                subject: $"Fwd: Automated Forward",
                body: $"This email was forwarded from NexusMail Automation.\nOriginal Email ID: {context.EmailId}",
                cancellationToken: cancellationToken
            );
            return ActionResult.Success;
        }
        catch (ArgumentException)
        {
            // Programming/validation errors are permanent
            return ActionResult.PermanentFailure;
        }
        catch (InvalidOperationException)
        {
            // Invalid states are permanent
            return ActionResult.PermanentFailure;
        }
        catch (Exception ex)
        {
            // If the provider fails with a network/SMTP issue, the outcome is UNKNOWN.
            // The side-effect MAY have completed. Returning TransientFailure would cause an unsafe blind retry.
            return ActionResult.Unknown;
        }
    }
}
