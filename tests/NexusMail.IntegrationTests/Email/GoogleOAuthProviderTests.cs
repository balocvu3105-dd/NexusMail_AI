using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NexusMail.Infrastructure.Authentication;
using Xunit;

namespace NexusMail.IntegrationTests.Email;

public class GoogleOAuthProviderTests
{
    [Fact]
    public async Task RefreshTokenAsync_MissingRefreshTokenInResponse_PreservesExistingRefreshToken()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(new HttpResponseMessage()
           {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent("{ \"access_token\": \"new_access\", \"expires_in\": 3600 }") // No refresh_token!
           })
           .Verifiable();
           
        var httpClient = new HttpClient(handlerMock.Object);

        var options = Options.Create(new GoogleOAuthOptions { ClientId = "id", ClientSecret = "secret" });
        var provider = new GoogleOAuthProvider(httpClient, options, NullLogger<GoogleOAuthProvider>.Instance);

        // Act
        var result = await provider.RefreshTokenAsync("old_refresh_token");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("new_access", result.Value.AccessToken);
        Assert.Equal("old_refresh_token", result.Value.RefreshToken); // Preserved!
    }

    [Fact]
    public async Task RefreshTokenAsync_NewRefreshTokenInResponse_ReplacesRefreshToken()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(new HttpResponseMessage()
           {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent("{ \"access_token\": \"new_access\", \"expires_in\": 3600, \"refresh_token\": \"new_refresh\" }")
           })
           .Verifiable();
           
        var httpClient = new HttpClient(handlerMock.Object);

        var options = Options.Create(new GoogleOAuthOptions { ClientId = "id", ClientSecret = "secret" });
        var provider = new GoogleOAuthProvider(httpClient, options, NullLogger<GoogleOAuthProvider>.Instance);

        // Act
        var result = await provider.RefreshTokenAsync("old_refresh_token");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("new_access", result.Value.AccessToken);
        Assert.Equal("new_refresh", result.Value.RefreshToken); // Replaced!
    }

    [Fact]
    public async Task RefreshTokenAsync_HttpFailure_ReturnsFailure()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(new HttpResponseMessage()
           {
               StatusCode = HttpStatusCode.BadRequest,
               Content = new StringContent("error")
           })
           .Verifiable();
           
        var httpClient = new HttpClient(handlerMock.Object);

        var options = Options.Create(new GoogleOAuthOptions { ClientId = "id", ClientSecret = "secret" });
        var provider = new GoogleOAuthProvider(httpClient, options, NullLogger<GoogleOAuthProvider>.Instance);

        // Act
        var result = await provider.RefreshTokenAsync("old_refresh_token");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("OAuthRefreshFailed", result.Error.Code);
    }
}
