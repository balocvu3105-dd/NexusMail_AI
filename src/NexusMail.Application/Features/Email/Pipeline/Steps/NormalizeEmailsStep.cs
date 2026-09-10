using Microsoft.Extensions.Logging;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Application.Features.Email.Pipeline.Models;
using NexusMail.Common.Constants;

namespace NexusMail.Application.Features.Email.Pipeline.Steps;

/// <summary>
/// Step 5 — Normalize ProviderMessages into the canonical NormalizedEmail model.
///
/// This step is the boundary between provider-specific data and domain data.
/// After normalization, the rest of the pipeline is provider-agnostic.
///
/// Writes: context.NormalizedEmails
/// </summary>
public sealed class NormalizeEmailsStep : ISyncPipelineStep
{
    private readonly IEmailMapper _mapper;
    private readonly ILogger<NormalizeEmailsStep> _logger;

    public string StepName => "NormalizeEmails";

    public NormalizeEmailsStep(IEmailMapper mapper, ILogger<NormalizeEmailsStep> logger)
    {
        _mapper = mapper;
        _logger = logger;
    }

    public Task ExecuteAsync(SyncContext context, CancellationToken cancellationToken = default)
    {
        if (context.ValidMessages.Count == 0)
        {
            context.NormalizedEmails = [];
            return Task.CompletedTask;
        }

        var normalized = new List<NormalizedEmail>(context.ValidMessages.Count);

        foreach (var message in context.ValidMessages)
        {
            try
            {
                var email = MapToNormalized(message, context.EmailAccountId);
                normalized.Add(email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[{Step}] Failed to normalize message {Id}. Skipping.", StepName, message.Id);
            }
        }

        context.NormalizedEmails = normalized.AsReadOnly();
        context.Result.Normalized = normalized.Count;

        _logger.LogDebug("[{Step}] Normalized {Count} emails.", StepName, normalized.Count);
        return Task.CompletedTask;
    }

    private static NormalizedEmail MapToNormalized(ProviderMessage message, Guid emailAccountId)
    {
        var (senderEmail, senderName) = message.SenderEmail != null && message.SenderName != null 
            ? (message.SenderEmail, message.SenderName)
            : ParseSender(message.Snippet);

        return new NormalizedEmail
        {
            MessageId = message.Id,
            ThreadId = message.ThreadId,
            Subject = message.Subject.Length > NexusConstants.Email.MaxSubjectLength
                ? message.Subject[..NexusConstants.Email.MaxSubjectLength]
                : message.Subject,
            Sender = message.SenderEmail ?? message.Snippet,
            SenderEmail = senderEmail,
            SenderName = senderName,
            Preview = message.Snippet.Length > NexusConstants.Email.MaxBodyPreviewLength
                ? message.Snippet[..NexusConstants.Email.MaxBodyPreviewLength]
                : message.Snippet,
            BodyText = message.Body,
            ReceivedAt = message.ReceivedAt ?? DateTimeOffset.UtcNow
        };
    }

    private static (string email, string name) ParseSender(string raw)
    {
        // Simple heuristic — Sprint 4 will use proper MIME parsing
        var atIndex = raw.IndexOf('@');
        if (atIndex < 0) return (raw, raw);

        var start = raw.LastIndexOf('<', atIndex);
        var end = raw.IndexOf('>', atIndex);

        if (start >= 0 && end > start)
        {
            var email = raw[(start + 1)..end].Trim();
            var name = raw[..start].Trim().Trim('"');
            return (email, name);
        }

        return (raw.Trim(), raw.Trim());
    }
}
