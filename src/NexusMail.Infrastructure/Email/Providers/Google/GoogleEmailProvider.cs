using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Features.Email.Abstractions;
using Microsoft.Extensions.Options;
using NexusMail.Infrastructure.Authentication;

namespace NexusMail.Infrastructure.Email.Providers.Google;

/// <summary>
/// Gmail API provider implementation.
/// Handles: message listing, full fetch, OAuth token refresh.
///
/// TODO Sprint 4:
///   - Implement incremental sync via historyId (delta sync)
///   - Proper MIME parsing for headers (From, To, Date)
///   - Attachment metadata extraction
///   - Retry-After header parsing
/// </summary>
public sealed class GoogleEmailProvider : IEmailProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleEmailProvider> _logger;
    private readonly GoogleOAuthOptions _options;

    private const string GmailBaseUrl = "https://gmail.googleapis.com/gmail/v1/users/me";

    public GoogleEmailProvider(HttpClient httpClient, ILogger<GoogleEmailProvider> logger, IOptions<GoogleOAuthOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<ProviderProfile> GetProfileAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{GmailBaseUrl}/profile");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var profileData = await response.Content.ReadFromJsonAsync<GoogleProfileResponse>(cancellationToken: cancellationToken);
        return new ProviderProfile(
            profileData?.EmailAddress ?? string.Empty,
            profileData?.HistoryId);
    }

    public Task<IReadOnlyList<ProviderLabel>> GetLabelsAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        // TODO Sprint 4: implement real Gmail labels API call
        IReadOnlyList<ProviderLabel> labels = new List<ProviderLabel>
        {
            new("INBOX", "Inbox"),
            new("SENT", "Sent"),
            new("DRAFT", "Drafts"),
            new("SPAM", "Spam"),
            new("TRASH", "Trash")
        };
        return Task.FromResult(labels);
    }

    public async Task<ProviderMessagePage> GetMessagesAsync(
        string accessToken, string? pageToken = null, string? cursor = null, CancellationToken cancellationToken = default)
    {
        string url;
        bool isHistory = false;
        
        if (!string.IsNullOrEmpty(cursor))
        {
            url = $"{GmailBaseUrl}/history?startHistoryId={cursor}&maxResults=100";
            isHistory = true;
        }
        else
        {
            // First-sync bootstrap: fetch last 30 days
            long thirtyDaysAgoUnix = ((DateTimeOffset)DateTime.UtcNow.AddDays(-30)).ToUnixTimeSeconds();
            url = $"{GmailBaseUrl}/messages?maxResults=100&q=after:{thirtyDaysAgoUnix}";
        }

        if (!string.IsNullOrEmpty(pageToken))
        {
            url += $"&pageToken={pageToken}";
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        
        if (isHistory && response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new NexusMail.Application.Features.Email.Exceptions.StaleHistoryIdException($"HistoryId {cursor} is stale or invalid.");
        }
        
        response.EnsureSuccessStatusCode();

        var summaries = new List<ProviderMessageSummary>();
        string? nextToken = null;
        string? nextCursor = null;

        if (isHistory)
        {
            var data = await response.Content.ReadFromJsonAsync<GoogleHistoryListResponse>(cancellationToken: cancellationToken);
            nextToken = data?.NextPageToken;
            nextCursor = data?.HistoryId; // this is the new historyId to save!
            
            if (data?.History != null)
            {
                foreach (var h in data.History)
                {
                    if (h.MessagesAdded != null)
                    {
                        foreach (var ma in h.MessagesAdded)
                        {
                            if (ma.Message?.Id != null)
                            {
                                summaries.Add(new ProviderMessageSummary(ma.Message.Id));
                            }
                        }
                    }
                }
            }
        }
        else
        {
            var data = await response.Content.ReadFromJsonAsync<GoogleMessagesListResponse>(cancellationToken: cancellationToken);
            nextToken = data?.NextPageToken;
            
            if (data?.Messages != null)
            {
                foreach (var m in data.Messages)
                {
                    if (m.Id != null) summaries.Add(new ProviderMessageSummary(m.Id));
                }
            }
        }

        return new ProviderMessagePage(summaries, nextToken, nextCursor);
    }

    public async Task<ProviderMessage> GetMessageAsync(
        string accessToken, string messageId, CancellationToken cancellationToken = default)
    {
        var url = $"{GmailBaseUrl}/messages/{messageId}?format=full";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<GoogleMessageFull>(cancellationToken: cancellationToken);
        
        string subject = "No Subject";
        string? senderEmail = null;
        string? senderName = null;
        DateTimeOffset? receivedAt = null;

        if (data?.Payload?.Headers != null)
        {
            foreach (var h in data.Payload.Headers)
            {
                if (h.Name == null || h.Value == null) continue;

                if (h.Name.Equals("Subject", System.StringComparison.OrdinalIgnoreCase))
                {
                    subject = h.Value;
                }
                else if (h.Name.Equals("From", System.StringComparison.OrdinalIgnoreCase))
                {
                    var (email, name) = ParseSender(h.Value);
                    senderEmail = email;
                    senderName = name;
                }
                else if (h.Name.Equals("Date", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (DateTimeOffset.TryParse(h.Value, out var dt))
                    {
                        receivedAt = dt.ToUniversalTime();
                    }
                }
            }
        }
        
        string body = ExtractBody(data?.Payload) ?? data?.Snippet ?? string.Empty;

        return new ProviderMessage(
            Id: messageId, 
            ThreadId: data?.ThreadId ?? messageId, 
            Subject: subject, 
            Snippet: data?.Snippet ?? string.Empty, 
            Body: body,
            SenderEmail: senderEmail,
            SenderName: senderName,
            ReceivedAt: receivedAt
        );
    }

    private static (string email, string name) ParseSender(string raw)
    {
        var atIndex = raw.IndexOf('@');
        if (atIndex < 0) return (raw, raw);

        var start = raw.LastIndexOf('<', atIndex);
        var end = raw.IndexOf('>', atIndex);

        if (start >= 0 && end > start)
        {
            var email = raw.Substring(start + 1, end - start - 1).Trim();
            var name = raw.Substring(0, start).Trim().Trim('"', ' ');
            return (email, string.IsNullOrWhiteSpace(name) ? email : name);
        }

        return (raw.Trim(), raw.Trim());
    }

    private static string? ExtractBody(GoogleMessagePayload? payload)
    {
        if (payload == null) return null;

        // Try to find plain text first
        var textPlain = FindPart(payload, "text/plain");
        if (textPlain?.Body?.Data != null)
        {
            return DecodeBase64Url(textPlain.Body.Data);
        }

        // Fallback to HTML, but sanitize it to plain text
        var textHtml = FindPart(payload, "text/html");
        if (textHtml?.Body?.Data != null)
        {
            var rawHtml = DecodeBase64Url(textHtml.Body.Data);
            return SanitizeHtmlToPlainText(rawHtml);
        }

        return null;
    }

    private static string SanitizeHtmlToPlainText(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        
        // Remove script and style elements and their contents
        var text = System.Text.RegularExpressions.Regex.Replace(html, @"<(script|style)[^>]*>[\s\S]*?</\1>", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // Very rudimentary HTML to text conversion for ingestion purposes
        // Replace <br> and <p> with newlines
        text = System.Text.RegularExpressions.Regex.Replace(text, @"<br\s*/?>", "\n", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        text = System.Text.RegularExpressions.Regex.Replace(text, @"</p>", "\n\n", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        // Strip all other HTML tags
        text = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]*>", string.Empty);
        
        // Decode common HTML entities
        text = System.Net.WebUtility.HtmlDecode(text);
        
        return text.Trim();
    }

    private static GoogleMessagePayload? FindPart(GoogleMessagePayload part, string mimeType)
    {
        if (part.MimeType?.Equals(mimeType, System.StringComparison.OrdinalIgnoreCase) == true && part.Body?.Data != null)
        {
            return part;
        }

        if (part.Parts != null)
        {
            foreach (var child in part.Parts)
            {
                var found = FindPart(child, mimeType);
                if (found != null) return found;
            }
        }

        return null;
    }

    private static string DecodeBase64Url(string base64Url)
    {
        try
        {
            string padded = base64Url.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            var bytes = System.Convert.FromBase64String(padded);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return string.Empty; // Fail safely on malformed encoding
        }
    }

    private sealed class GoogleHistoryListResponse
    {
        public List<GoogleHistoryRecord>? History { get; set; }
        public string? NextPageToken { get; set; }
        public string? HistoryId { get; set; }
    }

    private sealed class GoogleHistoryRecord
    {
        public string? Id { get; set; }
        public List<GoogleHistoryMessageAdded>? MessagesAdded { get; set; }
    }

    private sealed class GoogleHistoryMessageAdded
    {
        public GoogleMessageStub? Message { get; set; }
    }

    private sealed class GoogleMessageFull
    {
        public string? Id { get; set; }
        public string? ThreadId { get; set; }
        public string? Snippet { get; set; }
        public GoogleMessagePayload? Payload { get; set; }
    }

    private sealed class GoogleMessagePayload
    {
        [System.Text.Json.Serialization.JsonPropertyName("mimeType")]
        public string? MimeType { get; set; }
        
        public List<GoogleMessageHeader>? Headers { get; set; }
        
        public GoogleMessageBody? Body { get; set; }
        
        public List<GoogleMessagePayload>? Parts { get; set; }
    }

    private sealed class GoogleMessageBody
    {
        public int? Size { get; set; }
        public string? Data { get; set; }
        public string? AttachmentId { get; set; }
    }

    private sealed class GoogleMessageHeader
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
    }

    private sealed class GoogleMessagesListResponse
    {
        public List<GoogleMessageStub>? Messages { get; set; }
        public string? NextPageToken { get; set; }
        public int ResultSizeEstimate { get; set; }
    }

    private sealed class GoogleMessageStub
    {
        public string? Id { get; set; }
        public string? ThreadId { get; set; }
    }

    public async Task<ProviderTokens> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", _options.ClientId },
            { "client_secret", _options.ClientSecret },
            { "refresh_token", refreshToken },
            { "grant_type", "refresh_token" }
        });

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenData = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken: cancellationToken);
        if (tokenData == null || string.IsNullOrEmpty(tokenData.AccessToken))
        {
            throw new System.Exception("Failed to refresh Google token.");
        }

        return new ProviderTokens(tokenData.AccessToken, tokenData.ExpiresIn);
    }

    private sealed class GoogleTokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("scope")]
        public string? Scope { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
    }

    private sealed class GoogleProfileResponse
    {
        public string? EmailAddress { get; set; }
        public int? MessagesTotal { get; set; }
        public int? ThreadsTotal { get; set; }
        public string? HistoryId { get; set; }
    }
}
