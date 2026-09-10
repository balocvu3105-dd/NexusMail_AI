using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NexusMail.Infrastructure.Authentication;
using NexusMail.Infrastructure.Email.Providers.Google;
using Xunit;
using FluentAssertions;

namespace NexusMail.Infrastructure.Tests.Email;

public class GoogleEmailProviderTests
{
    [Fact]
    public async Task GetMessageAsync_ParsesHeadersAndDecodesBody_Correctly()
    {
        // Arrange
        var jsonResponse = """
        {
            "id": "msg123",
            "threadId": "thread123",
            "snippet": "This is a snippet",
            "payload": {
                "mimeType": "multipart/alternative",
                "headers": [
                    { "name": "Subject", "value": "Test Email" },
                    { "name": "From", "value": "\"John Doe\" <john@example.com>" },
                    { "name": "Date", "value": "Fri, 4 Sep 2026 10:00:00 +0000" }
                ],
                "parts": [
                    {
                        "mimeType": "text/html",
                        "body": { "data": "PGh0bWw-aWdub3JlZDwvaHRtbD4=" }
                    },
                    {
                        "mimeType": "text/plain",
                        "body": { "data": "SGVsbG8gV29ybGQ=" }
                    }
                ]
            }
        }
        """; // "SGVsbG8gV29ybGQ=" is "Hello World"
        
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var options = Options.Create(new GoogleOAuthOptions { ClientId = "test", ClientSecret = "test" });
        var provider = new GoogleEmailProvider(httpClient, NullLogger<GoogleEmailProvider>.Instance, options);

        // Act
        var message = await provider.GetMessageAsync("access_token", "msg123");

        // Assert
        message.Id.Should().Be("msg123");
        message.Subject.Should().Be("Test Email");
        message.SenderName.Should().Be("John Doe");
        message.SenderEmail.Should().Be("john@example.com");
        message.ReceivedAt.Should().Be(DateTimeOffset.Parse("Fri, 4 Sep 2026 10:00:00 +0000"));
        message.Body.Should().Be("Hello World");
    }
    
    [Fact]
    public async Task GetMessageAsync_FailsSafely_WhenBodyIsMalformed()
    {
        // Arrange
        var jsonResponse = """
        {
            "id": "msg123",
            "payload": {
                "mimeType": "text/plain",
                "body": { "data": "INVALID_BASE64_URL!!$$" }
            }
        }
        """; 
        
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var options = Options.Create(new GoogleOAuthOptions { ClientId = "test", ClientSecret = "test" });
        var provider = new GoogleEmailProvider(httpClient, NullLogger<GoogleEmailProvider>.Instance, options);

        // Act
        var message = await provider.GetMessageAsync("access_token", "msg123");

        // Assert
        message.Body.Should().BeEmpty(); // Fails safely
    }
}
