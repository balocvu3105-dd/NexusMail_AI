using Microsoft.Extensions.Logging;
using NexusMail.Application.Features.Email.Abstractions;

namespace NexusMail.Application.Features.Email.Pipeline.Steps;

/// <summary>
/// Step 1 — Fetch email message IDs from the email provider.
/// Handles token acquisition and paginated fetching.
/// Writes: context.AccessToken, context.FetchedMessageIds, context.NextPageToken
/// </summary>
public sealed class FetchEmailsStep : ISyncPipelineStep
{
    private readonly IEmailTokenService _tokenService;
    private readonly IEmailProviderFactory _providerFactory;
    private readonly ILogger<FetchEmailsStep> _logger;

    public string StepName => "FetchEmails";

    public FetchEmailsStep(
        IEmailTokenService tokenService,
        IEmailProviderFactory providerFactory,
        ILogger<FetchEmailsStep> logger)
    {
        _tokenService = tokenService;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync(SyncContext context, CancellationToken cancellationToken = default)
    {
        context.AccessToken = await _tokenService.GetValidAccessTokenAsync(
            context.EmailAccountId, cancellationToken);

        var provider = _providerFactory.GetProvider(context.Account.Provider);
        string? cursor = context.CurrentProviderCursor;
        string? pageToken = null;
        var allMessageIds = new List<ProviderMessageSummary>();
        string? finalCursor = null;

        if (string.IsNullOrEmpty(cursor))
        {
            var profile = await provider.GetProfileAsync(context.AccessToken, cancellationToken);
            context.BaselineCursor = profile.HistoryId;
        }

        while (true)
        {
            ProviderMessagePage page;
            try
            {
                page = await provider.GetMessagesAsync(context.AccessToken, pageToken, cursor, cancellationToken);
            }
            catch (NexusMail.Application.Features.Email.Exceptions.StaleHistoryIdException)
            {
                _logger.LogWarning("[{Step}] Stale HistoryId {Cursor} detected for account {AccountId}. Triggering re-bootstrap.", StepName, cursor, context.EmailAccountId);
                cursor = null;
                context.CurrentProviderCursor = null;
                pageToken = null;
                allMessageIds.Clear();
                
                var profile = await provider.GetProfileAsync(context.AccessToken, cancellationToken);
                context.BaselineCursor = profile.HistoryId;
                
                page = await provider.GetMessagesAsync(context.AccessToken, pageToken, cursor, cancellationToken);
            }

            allMessageIds.AddRange(page.Messages);
            pageToken = page.NextPageToken;
            
            if (!string.IsNullOrEmpty(page.NextCursor))
            {
                finalCursor = page.NextCursor;
            }

            if (string.IsNullOrEmpty(pageToken))
            {
                break; // No more pages
            }
        }

        context.FetchedMessageIds = allMessageIds.AsReadOnly();
        context.NextPageToken = null; // Exhausted
        
        if (!string.IsNullOrEmpty(finalCursor))
        {
            context.NextProviderCursor = finalCursor;
        }
        else if (string.IsNullOrEmpty(cursor) && !string.IsNullOrEmpty(context.BaselineCursor))
        {
            context.NextProviderCursor = context.BaselineCursor;
        }

        context.Result.Fetched = allMessageIds.Count;

        _logger.LogDebug(
            "[{Step}] Fetched {Count} message IDs for account {AccountId}.",
            StepName, allMessageIds.Count, context.EmailAccountId);
    }
}
