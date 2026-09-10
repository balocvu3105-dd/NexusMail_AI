using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Domain.Email.Enums;
using NexusMail.Domain.Email.Exceptions;
using NexusMail.Shared.Common;

namespace NexusMail.Infrastructure.Authentication;

public sealed class GoogleOAuthProvider : IOAuthProvider
{
    private readonly HttpClient _httpClient;
    private readonly GoogleOAuthOptions _options;
    private readonly ILogger<GoogleOAuthProvider> _logger;

    public GoogleOAuthProvider(HttpClient httpClient, IOptions<GoogleOAuthOptions> options, ILogger<GoogleOAuthProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public EmailProvider Provider => EmailProvider.Google;

    public async Task<Result<OAuthTokens>> ExchangeCodeAsync(string authCode, string redirectUri, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(authCode))
        {
            return Result.Failure<OAuthTokens>(new Error("InvalidAuthCode", "Authorization code cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            return Result.Failure<OAuthTokens>(new Error("MissingGoogleOAuthConfiguration", "Google OAuth ClientId or ClientSecret is not configured."));
        }

        try
        {
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", _options.ClientId },
                    { "client_secret", _options.ClientSecret },
                    { "code", authCode },
                    { "grant_type", "authorization_code" },
                    { "redirect_uri", redirectUri }
                })
            };

            var tokenResponse = await _httpClient.SendAsync(tokenRequest, cancellationToken);
            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorContent = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to exchange auth code. Status: {Status}, Response: {Response}", tokenResponse.StatusCode, errorContent);
                return Result.Failure<OAuthTokens>(new Error("OAuthExchangeFailed", "Failed to exchange authorization code with Google."));
            }

            var tokenData = await tokenResponse.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken: cancellationToken);
            if (tokenData == null || string.IsNullOrWhiteSpace(tokenData.AccessToken))
            {
                return Result.Failure<OAuthTokens>(new Error("InvalidTokenResponse", "Google token response is invalid."));
            }

            // Fetch profile to get email address
            var profileRequest = new HttpRequestMessage(HttpMethod.Get, "https://gmail.googleapis.com/gmail/v1/users/me/profile");
            profileRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenData.AccessToken);
            
            var profileResponse = await _httpClient.SendAsync(profileRequest, cancellationToken);
            if (!profileResponse.IsSuccessStatusCode)
            {
                var errorContent = await profileResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to fetch Gmail profile. Status: {Status}, Response: {Response}", profileResponse.StatusCode, errorContent);
                return Result.Failure<OAuthTokens>(new Error("ProfileFetchFailed", "Failed to fetch user profile from Google."));
            }

            var profileData = await profileResponse.Content.ReadFromJsonAsync<GoogleProfileResponse>(cancellationToken: cancellationToken);
            if (profileData == null || string.IsNullOrWhiteSpace(profileData.EmailAddress))
            {
                return Result.Failure<OAuthTokens>(new Error("InvalidProfileResponse", "Google profile response did not contain an email address."));
            }

            var tokens = new OAuthTokens(
                AccessToken: tokenData.AccessToken,
                RefreshToken: tokenData.RefreshToken ?? string.Empty,
                ExpiresAt: DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn),
                EmailAddress: profileData.EmailAddress
            );

            return Result.Success(tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during Google OAuth exchange.");
            return Result.Failure<OAuthTokens>(new Error("OAuthExchangeException", "An error occurred during OAuth exchange."));
        }
    }

    public async Task<Result<OAuthTokens>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Failure<OAuthTokens>(new Error("InvalidRefreshToken", "Refresh token cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            return Result.Failure<OAuthTokens>(new Error("MissingGoogleOAuthConfiguration", "Google OAuth ClientId or ClientSecret is not configured."));
        }

        try
        {
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", _options.ClientId },
                    { "client_secret", _options.ClientSecret },
                    { "refresh_token", refreshToken },
                    { "grant_type", "refresh_token" }
                })
            };

            var tokenResponse = await _httpClient.SendAsync(tokenRequest, cancellationToken);
            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorContent = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to refresh token. Status: {Status}, Response: {Response}", tokenResponse.StatusCode, errorContent);
                return Result.Failure<OAuthTokens>(new Error("OAuthRefreshFailed", "Failed to refresh token with Google."));
            }

            var tokenData = await tokenResponse.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken: cancellationToken);
            if (tokenData == null || string.IsNullOrWhiteSpace(tokenData.AccessToken))
            {
                return Result.Failure<OAuthTokens>(new Error("InvalidTokenResponse", "Google token response is invalid."));
            }

            // Important: Google might not return a new refresh token. If missing, keep the old one.
            var tokens = new OAuthTokens(
                AccessToken: tokenData.AccessToken,
                RefreshToken: !string.IsNullOrWhiteSpace(tokenData.RefreshToken) ? tokenData.RefreshToken : refreshToken,
                ExpiresAt: DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn),
                EmailAddress: string.Empty // Email address is not usually returned or needed during refresh
            );

            return Result.Success(tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during Google OAuth token refresh.");
            return Result.Failure<OAuthTokens>(new Error("OAuthRefreshException", "An error occurred during OAuth token refresh."));
        }
    }

    private sealed class GoogleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
        
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    private sealed class GoogleProfileResponse
    {
        [JsonPropertyName("emailAddress")]
        public string? EmailAddress { get; set; }
    }
}
