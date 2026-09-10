using Microsoft.Extensions.Logging;
using NexusMail.Application.Features.Email.Abstractions;

namespace NexusMail.Application.Features.Email.Pipeline.Steps;

/// <summary>
/// Step 4 — Fetch full message content and validate required fields.
/// Filters out malformed or incomplete messages before normalization.
/// Writes: context.ValidMessages, context.InvalidCount
/// </summary>
public sealed class ValidateEmailsStep : ISyncPipelineStep
{
    private readonly IEmailProviderFactory _providerFactory;
    private readonly ILogger<ValidateEmailsStep> _logger;

    public string StepName => "ValidateEmails";

    public ValidateEmailsStep(
        IEmailProviderFactory providerFactory,
        ILogger<ValidateEmailsStep> logger)
    {
        _providerFactory = providerFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync(SyncContext context, CancellationToken cancellationToken = default)
    {
        if (context.FetchedMessageIds.Count == 0)
        {
            _logger.LogDebug("[{Step}] No messages to validate.", StepName);
            return;
        }

        var provider = _providerFactory.GetProvider(context.Account.Provider);
        var validMessages = new List<ProviderMessage>();
        var invalidCount = 0;

        // Fetch full message details for each ID
        foreach (var summary in context.FetchedMessageIds)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                var message = await provider.GetMessageAsync(
                    context.AccessToken!, summary.Id, cancellationToken);

                if (IsValid(message))
                    validMessages.Add(message);
                else
                {
                    invalidCount++;
                    _logger.LogWarning("[{Step}] Skipping invalid message {Id}: missing required fields.", StepName, summary.Id);
                }
            }
            catch (Exception ex)
            {
                invalidCount++;
                _logger.LogWarning(ex, "[{Step}] Failed to fetch message {Id}. Skipping.", StepName, summary.Id);
            }
        }

        context.ValidMessages = validMessages.AsReadOnly();
        context.InvalidCount = invalidCount;
        context.Result.Valid = validMessages.Count;
        context.Result.Invalid = invalidCount;

        _logger.LogDebug("[{Step}] Valid={Valid} Invalid={Invalid}", StepName, validMessages.Count, invalidCount);
    }

    private static bool IsValid(ProviderMessage message)
        => !string.IsNullOrWhiteSpace(message.Id)
        && !string.IsNullOrWhiteSpace(message.ThreadId)
        && !string.IsNullOrWhiteSpace(message.Subject);
}
