using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Application.Abstractions.Integration;
using NexusMail.Contracts.Automation;
using NexusMail.Domain.Automation.Enums;

namespace NexusMail.Automation.Actions;

public sealed class CreateTaskActionExecutor : IActionExecutor
{
    private readonly ITaskManagementProvider _taskManagementProvider;

    public ActionType ActionType => ActionType.CreateTask;

    public CreateTaskActionExecutor(ITaskManagementProvider taskManagementProvider)
    {
        _taskManagementProvider = taskManagementProvider;
    }

    public async Task<ActionResult> ExecuteAsync(string idempotencyKey, string parametersJson, EvaluateRulesMessage context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(parametersJson) || parametersJson == "[]" || parametersJson == "{}")
        {
            return ActionResult.PermanentFailure;
        }

        string provider = "";
        string title = "";
        string? description = null;
        string? assignee = null;

        try
        {
            var doc = JsonDocument.Parse(parametersJson);
            if (doc.RootElement.TryGetProperty("provider", out var providerProp) && providerProp.ValueKind == JsonValueKind.String)
            {
                provider = providerProp.GetString() ?? "";
            }
            if (doc.RootElement.TryGetProperty("title", out var titleProp) && titleProp.ValueKind == JsonValueKind.String)
            {
                title = titleProp.GetString() ?? "";
            }
            if (doc.RootElement.TryGetProperty("description", out var descProp) && descProp.ValueKind == JsonValueKind.String)
            {
                description = descProp.GetString();
            }
            if (doc.RootElement.TryGetProperty("assignee", out var assignProp) && assignProp.ValueKind == JsonValueKind.String)
            {
                assignee = assignProp.GetString();
            }
        }
        catch (JsonException)
        {
            return ActionResult.PermanentFailure;
        }

        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(title))
        {
            return ActionResult.PermanentFailure;
        }

        try
        {
            var parameters = new CreateTaskParameters(provider, title, description, assignee);
            var taskId = await _taskManagementProvider.CreateTaskAsync(parameters, idempotencyKey, cancellationToken);
            
            if (!string.IsNullOrWhiteSpace(taskId))
            {
                return ActionResult.Success;
            }
            
            return ActionResult.PermanentFailure;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // By default, assume external calls failing randomly are transient
            return ActionResult.TransientFailure;
        }
    }
}
