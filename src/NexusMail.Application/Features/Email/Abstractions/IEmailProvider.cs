using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NexusMail.Application.Features.Email.Abstractions;

public interface IEmailProvider
{
    Task<ProviderProfile> GetProfileAsync(string accessToken, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<ProviderLabel>> GetLabelsAsync(string accessToken, CancellationToken cancellationToken = default);
    
    Task<ProviderMessagePage> GetMessagesAsync(string accessToken, string? pageToken = null, string? cursor = null, CancellationToken cancellationToken = default);
    
    Task<ProviderMessage> GetMessageAsync(string accessToken, string messageId, CancellationToken cancellationToken = default);
    
    Task<ProviderTokens> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}

public record ProviderProfile(string EmailAddress, string? HistoryId);
public record ProviderLabel(string Id, string Name);
public record ProviderMessagePage(IReadOnlyList<ProviderMessageSummary> Messages, string? NextPageToken, string? NextCursor = null);
public record ProviderMessageSummary(string Id);
public record ProviderMessage(
    string Id, 
    string ThreadId, 
    string Subject, 
    string Snippet, 
    string Body,
    string? SenderEmail = null,
    string? SenderName = null,
    System.DateTimeOffset? ReceivedAt = null
);
public record ProviderTokens(string AccessToken, int ExpiresInSeconds);
