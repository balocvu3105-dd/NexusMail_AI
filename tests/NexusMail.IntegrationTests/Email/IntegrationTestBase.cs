using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;
using Xunit;
using NexusMail.Infrastructure.Persistence;
using NexusMail.Application;
using NexusMail.Infrastructure;
using MassTransit;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Application.Features.Email.Pipeline;
using NexusMail.Application.Features.Email.Services;
using NexusMail.Infrastructure.Email.Providers;
using NexusMail.Infrastructure.Email.Providers.Fake;
using Microsoft.Extensions.Configuration;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace NexusMail.IntegrationTests.Email;

public class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    protected IServiceProvider ServiceProvider { get; private set; }
    protected ApplicationDbContext DbContext { get; private set; }
    protected IServiceScope Scope { get; private set; }

    public IntegrationTestBase()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("nexusmail_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var services = new ServiceCollection();

        // Logging
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        // MassTransit (Test Harness)
        services.AddMassTransitTestHarness();

        // MediatR
        services.AddApplication();
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(typeof(NexusMail.EventHandlers.Email.EmailReceivedDomainEventHandler).Assembly));

        // Infrastructure (without AI to keep it lightweight)
        services.AddScoped<NexusMail.Infrastructure.Persistence.Interceptors.ConvertDomainEventsToOutboxMessagesInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<NexusMail.Infrastructure.Persistence.Interceptors.ConvertDomainEventsToOutboxMessagesInterceptor>();
            options.UseNpgsql(_dbContainer.GetConnectionString())
                   .AddInterceptors(interceptor);
        });

        services.AddScoped<IEmailAccountRepository, NexusMail.Infrastructure.Persistence.Repositories.EmailAccountRepository>();
        
        // Wait, EmailPersistenceService is private in DependencyInjection.cs. I need to make it public or use AddInfrastructure
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = _dbContainer.GetConnectionString(),
                ["AI:OpenAI:ApiKey"] = "fake-key"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddInfrastructure(configuration);
            
        // Override with test specific variants
        services.AddScoped<IEmailProviderFactory, TestEmailProviderFactory>();
        services.AddScoped<IEmailTokenService, TestEmailTokenService>();
        services.AddScoped<FakeEmailProvider>();

        ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();

        // Apply migrations
        Scope = ServiceProvider.CreateScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await DbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        Scope?.Dispose();
        await _dbContainer.DisposeAsync();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }
}

public class TestEmailProviderFactory : IEmailProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    public IEmailProvider? OverrideProvider { get; set; }

    public TestEmailProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEmailProvider GetProvider(NexusMail.Domain.Email.Enums.EmailProvider provider)
    {
        if (OverrideProvider != null)
        {
            return OverrideProvider;
        }
        return _serviceProvider.GetRequiredService<FakeEmailProvider>();
    }

    public bool IsSupported(NexusMail.Domain.Email.Enums.EmailProvider provider)
    {
        return true;
    }
}

public class TestEmailTokenService : IEmailTokenService
{
    public Task<string> GetValidAccessTokenAsync(Guid accountId, System.Threading.CancellationToken cancellationToken = default)
    {
        return Task.FromResult("test-token");
    }
}
