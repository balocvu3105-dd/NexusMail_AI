using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using NexusMail.Infrastructure.Authentication;

namespace NexusMail.IntegrationTests.Identity;

public class OAuthEndpointsTests : IClassFixture<CustomWebApplicationFactory<NexusMail.API.ApiMarker>>
{
    private const string StateProtectorPurpose = "NexusMail.OAuth.State";
    private readonly CustomWebApplicationFactory<NexusMail.API.ApiMarker> _factory;

    public OAuthEndpointsTests(CustomWebApplicationFactory<NexusMail.API.ApiMarker> factory)
    {
        _factory = factory;
    }

    private record OAuthStatePayload(Guid UserId, Guid WorkspaceId, DateTimeOffset ExpiresAt, string CsrfToken);

    [Fact]
    public async Task Callback_WithInvalidState_ReturnsRedirectWithError()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Act
        var response = await client.GetAsync("/api/v1/auth/google/callback?code=testcode&state=invalid_state");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("error=invalid_state");
    }

    [Fact]
    public async Task Callback_WithExpiredState_ReturnsRedirectWithError()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var dataProtector = _factory.Services.GetRequiredService<IDataProtectionProvider>();
        var protector = dataProtector.CreateProtector(StateProtectorPurpose);

        var payload = new OAuthStatePayload(
            UserId: Guid.NewGuid(),
            WorkspaceId: Guid.NewGuid(),
            ExpiresAt: DateTimeOffset.UtcNow.AddMinutes(-5), // Expired
            CsrfToken: "testcsrf"
        );

        var state = protector.Protect(JsonSerializer.Serialize(payload));
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/auth/google/callback?code=testcode&state={Uri.EscapeDataString(state)}");
        request.Headers.Add("Cookie", "oauth_csrf=testcsrf");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("error=state_expired");
    }

    [Fact]
    public async Task Callback_WithInvalidCsrf_ReturnsRedirectWithError()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var dataProtector = _factory.Services.GetRequiredService<IDataProtectionProvider>();
        var protector = dataProtector.CreateProtector(StateProtectorPurpose);

        var payload = new OAuthStatePayload(
            UserId: Guid.NewGuid(),
            WorkspaceId: Guid.NewGuid(),
            ExpiresAt: DateTimeOffset.UtcNow.AddMinutes(5),
            CsrfToken: "expectedcsrf"
        );

        var state = protector.Protect(JsonSerializer.Serialize(payload));
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/auth/google/callback?code=testcode&state={Uri.EscapeDataString(state)}");
        request.Headers.Add("Cookie", "oauth_csrf=wrongcsrf");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("error=csrf_failed");
    }

    [Fact]
    public async Task Callback_WithUnauthorizedWorkspace_ReturnsRedirectWithError()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var dataProtector = _factory.Services.GetRequiredService<IDataProtectionProvider>();
        var protector = dataProtector.CreateProtector(StateProtectorPurpose);

        // Create a test user but do NOT add them to the workspace
        var userId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid(); // Workspace does not exist

        var payload = new OAuthStatePayload(
            UserId: userId,
            WorkspaceId: workspaceId,
            ExpiresAt: DateTimeOffset.UtcNow.AddMinutes(5),
            CsrfToken: "testcsrf"
        );

        var state = protector.Protect(JsonSerializer.Serialize(payload));
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/auth/google/callback?code=testcode&state={Uri.EscapeDataString(state)}");
        request.Headers.Add("Cookie", "oauth_csrf=testcsrf");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("error=unauthorized_workspace");
    }
}
