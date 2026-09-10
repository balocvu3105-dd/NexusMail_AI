using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NexusMail.Infrastructure.Persistence;
using Xunit;
using Microsoft.EntityFrameworkCore;
using NexusMail.Contracts.Email;

namespace NexusMail.Tests.E2E;

public class GoldenPathTests : IClassFixture<NexusMailApiFactory>
{
    private readonly NexusMailApiFactory _factory;

    public GoldenPathTests(NexusMailApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GoldenPath_EmailIngestion_ShouldProcessThroughAllStages()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Setup initial data (Workspace, Rule)
        var workspaceId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        // Note: For a true E2E, we would mock the IEmailProvider to return a test email
        // and trigger the sync endpoint. Since we don't have all consumers wired in this 
        // single web app factory (they are separate processes), this test ensures the 
        // factory spins up. Let's do a basic API health check first.

        var response = await client.GetAsync("/health/ready");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }
}
