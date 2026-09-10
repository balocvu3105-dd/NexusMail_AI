using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using NexusMail.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace NexusMail.Tests.E2E;

public class NexusMailApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;

    public NexusMailApiFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            // We use pgvector/pgvector:pg16 instead of regular postgres for pgvector support
            .WithImage("pgvector/pgvector:pg16")
            .WithDatabase("nexusmail_test")
            .WithUsername("postgres")
            .WithPassword("password")
            .Build();
            
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", _dbContainer.GetConnectionString()),
                new KeyValuePair<string, string?>("ConnectionStrings:RabbitMq", _rabbitMqContainer.GetConnectionString()),
                new KeyValuePair<string, string?>("FeatureFlags:AiEnabled", "true"),
                new KeyValuePair<string, string?>("FeatureFlags:AutomationEnabled", "true"),
                new KeyValuePair<string, string?>("FeatureFlags:SearchEnabled", "true")
            });
        });

        builder.ConfigureServices(services =>
        {
            // Reconfigure anything specifically for E2E tests if necessary
            // MassTransit will automatically use the updated RabbitMq connection string from configuration
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();

        // Run migrations
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Wait for Postgres to be fully ready
        await Task.Delay(1000); 
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _rabbitMqContainer.StopAsync();
    }
}
