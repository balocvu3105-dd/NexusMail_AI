using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Application.Abstractions.Integration;
using NexusMail.Contracts.Automation;
using NexusMail.Domain.Automation.Enums;

namespace NexusMail.Automation.Actions;

public sealed class SaveAttachmentsActionExecutor : IActionExecutor
{
    private readonly IAttachmentSource _attachmentSource;
    private readonly IAttachmentStorageProvider _storageProvider;

    public ActionType ActionType => ActionType.SaveAttachments;

    public SaveAttachmentsActionExecutor(IAttachmentSource attachmentSource, IAttachmentStorageProvider storageProvider)
    {
        _attachmentSource = attachmentSource;
        _storageProvider = storageProvider;
    }

    public async Task<ActionResult> ExecuteAsync(string idempotencyKey, string parametersJson, EvaluateRulesMessage context, CancellationToken cancellationToken = default)
    {
        if (!context.HasAttachments)
        {
            // By semantics, if there are no attachments, we can just say success (nothing to do)
            // or we could say PermanentFailure. But "Save attachments" on an email with no attachments
            // logically succeeds trivially (0 files saved).
            return ActionResult.Success;
        }

        if (string.IsNullOrWhiteSpace(parametersJson) || parametersJson == "[]" || parametersJson == "{}")
        {
            return ActionResult.PermanentFailure;
        }

        string provider = "";
        string destinationPath = "";

        try
        {
            var doc = JsonDocument.Parse(parametersJson);
            if (doc.RootElement.TryGetProperty("provider", out var providerProp) && providerProp.ValueKind == JsonValueKind.String)
            {
                provider = providerProp.GetString() ?? "";
            }
            if (doc.RootElement.TryGetProperty("destinationPath", out var pathProp) && pathProp.ValueKind == JsonValueKind.String)
            {
                destinationPath = pathProp.GetString() ?? "";
            }
        }
        catch (JsonException)
        {
            return ActionResult.PermanentFailure;
        }

        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(destinationPath))
        {
            return ActionResult.PermanentFailure;
        }

        try
        {
            var attachments = await _attachmentSource.GetAttachmentsAsync(context.WorkspaceId, context.EmailId, cancellationToken);

            if (attachments == null || attachments.Count == 0)
            {
                return ActionResult.Success; // Trivial success
            }

            bool allSaved = true;

            foreach (var attachment in attachments)
            {
                // Note: idempotencyKey could be appended with attachment.Id to make it unique per file
                string fileIdempotencyKey = $"{idempotencyKey}:{attachment.Id}";
                var saved = await _storageProvider.SaveAsync(attachment, destinationPath, fileIdempotencyKey, cancellationToken);
                
                // It is important to close or dispose streams if required, 
                // but we assume IAttachmentSource provides fresh streams and we'll dispose them here to be safe.
                await attachment.ContentStream.DisposeAsync();

                if (!saved)
                {
                    allSaved = false;
                }
            }

            return allSaved ? ActionResult.Success : ActionResult.TransientFailure;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // Transient failure for network/storage errors
            return ActionResult.TransientFailure;
        }
    }
}
