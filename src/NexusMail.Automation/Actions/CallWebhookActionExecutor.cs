using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Contracts.Automation;
using NexusMail.Domain.Automation.Enums;

namespace NexusMail.Automation.Actions;

public sealed class CallWebhookActionExecutor : IActionExecutor
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ActionType ActionType => ActionType.CallWebhook;

    public CallWebhookActionExecutor(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ActionResult> ExecuteAsync(string idempotencyKey, string parametersJson, EvaluateRulesMessage context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(parametersJson) || parametersJson == "[]" || parametersJson == "{}")
        {
            return ActionResult.PermanentFailure;
        }

        string url = "";
        string method = "POST";

        try
        {
            var doc = JsonDocument.Parse(parametersJson);
            if (doc.RootElement.TryGetProperty("url", out var urlProp) && urlProp.ValueKind == JsonValueKind.String)
            {
                url = urlProp.GetString() ?? "";
            }
            if (doc.RootElement.TryGetProperty("method", out var methodProp) && methodProp.ValueKind == JsonValueKind.String)
            {
                method = methodProp.GetString() ?? "POST";
            }
        }
        catch (JsonException)
        {
            return ActionResult.PermanentFailure;
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            return ActionResult.PermanentFailure;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return ActionResult.PermanentFailure;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return ActionResult.PermanentFailure;
        }

        var requestMethod = method.ToUpperInvariant() switch
        {
            "GET" => HttpMethod.Get,
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            "PATCH" => HttpMethod.Patch,
            "DELETE" => HttpMethod.Delete,
            _ => null
        };

        if (requestMethod == null)
        {
            return ActionResult.PermanentFailure;
        }

        using var request = new HttpRequestMessage(requestMethod, uri);
        request.Headers.Add("Idempotency-Key", idempotencyKey);

        if (requestMethod != HttpMethod.Get)
        {
            var payload = new
            {
                EmailId = context.EmailId,
                WorkspaceId = context.WorkspaceId,
                Subject = context.Subject,
                Sender = context.Sender,
                ReceivedAt = context.ReceivedAt
            };
            var jsonPayload = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        }

        try
        {
            using var client = _httpClientFactory.CreateClient("WebhookClient");
            using var response = await client.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return ActionResult.Success;
            }

            var statusCode = (int)response.StatusCode;
            if (statusCode >= 400 && statusCode < 500)
            {
                return ActionResult.PermanentFailure;
            }
            if (statusCode >= 500)
            {
                return ActionResult.TransientFailure;
            }

            return ActionResult.PermanentFailure;
        }
        catch (OperationCanceledException)
        {
            // Propagate cancellation
            throw;
        }
        catch (HttpRequestException)
        {
            // Network error
            return ActionResult.TransientFailure;
        }
        catch (Exception)
        {
            return ActionResult.TransientFailure;
        }
    }
}
