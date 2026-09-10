using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NexusMail.Application.Features.Email.Exceptions;
using NexusMail.Infrastructure.Authentication;
using NexusMail.Infrastructure.Email.Providers.Google;
using Xunit;

namespace NexusMail.Infrastructure.Tests.Email.Providers.Google;

public class GoogleEmailProviderTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<GoogleEmailProvider>> _loggerMock;
    private readonly IOptions<GoogleOAuthOptions> _options;
    private readonly GoogleEmailProvider _provider;

    public GoogleEmailProviderTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _loggerMock = new Mock<ILogger<GoogleEmailProvider>>();
        
        var options = new GoogleOAuthOptions 
        { 
            ClientId = "test-client", 
            ClientSecret = "test-secret" 
        };
        _options = Options.Create(options);

        _provider = new GoogleEmailProvider(_httpClient, _loggerMock.Object, _options);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string jsonContent)
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
            });
    }

    [Fact]
    public async Task GetProfileAsync_ShouldReturnProfile()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"emailAddress\": \"test@example.com\", \"historyId\": \"12345\"}");

        // Act
        var result = await _provider.GetProfileAsync("token");

        // Assert
        result.EmailAddress.Should().Be("test@example.com");
        result.HistoryId.Should().Be("12345");
    }

    [Fact]
    public async Task GetMessagesAsync_InitialSync_ShouldReturnMessages()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"messages\": [{\"id\": \"msg1\"}, {\"id\": \"msg2\"}], \"nextPageToken\": \"page2\"}");

        // Act
        var result = await _provider.GetMessagesAsync("token");

        // Assert
        result.Messages.Should().HaveCount(2);
        result.Messages[0].Id.Should().Be("msg1");
        result.Messages[1].Id.Should().Be("msg2");
        result.NextPageToken.Should().Be("page2");
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task GetMessagesAsync_IncrementalSync_ShouldReturnMessagesAndHistoryId()
    {
        // Arrange
        var json = @"
        {
            ""historyId"": ""99999"",
            ""history"": [
                {
                    ""messagesAdded"": [
                        { ""message"": { ""id"": ""msg3"" } }
                    ]
                }
            ]
        }";
        SetupHttpResponse(HttpStatusCode.OK, json);

        // Act
        var result = await _provider.GetMessagesAsync("token", cursor: "12345");

        // Assert
        result.Messages.Should().HaveCount(1);
        result.Messages[0].Id.Should().Be("msg3");
        result.NextCursor.Should().Be("99999");
    }

    [Fact]
    public async Task GetMessagesAsync_IncrementalSync_ShouldHandlePagination()
    {
        // Arrange
        var page1Json = @"
        {
            ""historyId"": ""99999"",
            ""nextPageToken"": ""page2"",
            ""history"": [
                {
                    ""messagesAdded"": [
                        { ""message"": { ""id"": ""msg1"" } }
                    ]
                }
            ]
        }";
        SetupHttpResponse(HttpStatusCode.OK, page1Json);

        // Act
        var result = await _provider.GetMessagesAsync("token", cursor: "12345");

        // Assert
        result.Messages.Should().HaveCount(1);
        result.Messages[0].Id.Should().Be("msg1");
        result.NextCursor.Should().Be("99999");
        result.NextPageToken.Should().Be("page2");
    }

    [Fact]
    public async Task GetMessagesAsync_IncrementalSync_StaleHistoryId_ShouldThrow()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NotFound, "{}");

        // Act & Assert
        await Assert.ThrowsAsync<StaleHistoryIdException>(() => 
            _provider.GetMessagesAsync("token", cursor: "stale-cursor"));
    }

    [Fact]
    public async Task GetMessageAsync_ShouldParseHeadersAndPlainText()
    {
        // Arrange
        var plainTextB64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("Hello Plain Text"));
        var plainTextSafe = plainTextB64.Replace('+', '-').Replace('/', '_');
        
        var json = $@"
        {{
            ""id"": ""msg1"",
            ""threadId"": ""thread1"",
            ""snippet"": ""Hello snippet"",
            ""payload"": {{
                ""headers"": [
                    {{ ""name"": ""Subject"", ""value"": ""Test Subject"" }},
                    {{ ""name"": ""From"", ""value"": ""John Doe <john@example.com>"" }},
                    {{ ""name"": ""Date"", ""value"": ""Thu, 10 Sep 2026 10:00:00 +0000"" }}
                ],
                ""parts"": [
                    {{
                        ""mimeType"": ""text/plain"",
                        ""body"": {{ ""data"": ""{plainTextSafe}"" }}
                    }}
                ]
            }}
        }}";
        SetupHttpResponse(HttpStatusCode.OK, json);

        // Act
        var result = await _provider.GetMessageAsync("token", "msg1");

        // Assert
        result.Subject.Should().Be("Test Subject");
        result.SenderName.Should().Be("John Doe");
        result.SenderEmail.Should().Be("john@example.com");
        result.ReceivedAt.Should().Be(DateTimeOffset.Parse("2026-09-10T10:00:00Z"));
        result.Body.Should().Be("Hello Plain Text");
    }

    [Fact]
    public async Task GetMessageAsync_ShouldFallbackToHtmlAndSanitize()
    {
        // Arrange
        var htmlB64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("<html><body>Hello <br> HTML <p>paragraph</p> <script>alert(1)</script></body></html>"));
        var htmlSafe = htmlB64.Replace('+', '-').Replace('/', '_');
        
        var json = $@"
        {{
            ""id"": ""msg2"",
            ""payload"": {{
                ""parts"": [
                    {{
                        ""mimeType"": ""text/html"",
                        ""body"": {{ ""data"": ""{htmlSafe}"" }}
                    }}
                ]
            }}
        }}";
        SetupHttpResponse(HttpStatusCode.OK, json);

        // Act
        var result = await _provider.GetMessageAsync("token", "msg2");

        // Assert
        result.Body.Should().Be("Hello \n HTML paragraph"); // Tags stripped, <br> to \n, <p> to \n\n, script and style removed entirely
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNewAccessToken()
    {
        // Arrange
        var json = @"
        {
            ""access_token"": ""new-access-token"",
            ""expires_in"": 3599,
            ""scope"": ""https://mail.google.com/"",
            ""token_type"": ""Bearer""
        }";
        SetupHttpResponse(HttpStatusCode.OK, json);

        // Act
        var result = await _provider.RefreshTokenAsync("refresh-token");

        // Assert
        result.AccessToken.Should().Be("new-access-token");
        result.ExpiresInSeconds.Should().Be(3599);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrowOnHttpError()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.BadRequest, "{}");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            _provider.RefreshTokenAsync("refresh-token"));
    }

    [Fact]
    public async Task GetMessageAsync_ShouldThrowOnHttpError()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.InternalServerError, "{}");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            _provider.GetMessageAsync("token", "msg1"));
    }
}
