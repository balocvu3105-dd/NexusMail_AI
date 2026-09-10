using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using MassTransit;
using NexusMail.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;

using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Xunit;

namespace NexusMail.IntegrationTests
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime where TProgram : class
    {
        private readonly PostgreSqlContainer _dbContainer;

        public CustomWebApplicationFactory()
        {
            _dbContainer = new PostgreSqlBuilder()
                .WithImage("postgres:15-alpine")
                .WithDatabase("nexusmail_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            System.Environment.SetEnvironmentVariable("RateLimiting__LoginLimit", "1000");
            System.Environment.SetEnvironmentVariable("RateLimiting__RefreshLimit", "1000");
            System.Environment.SetEnvironmentVariable("RateLimiting__LoginWindowSeconds", "1");
            System.Environment.SetEnvironmentVariable("RateLimiting__RefreshWindowSeconds", "1");
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
            using var scope = this.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptors1 = services.Where(d => d.ServiceType == typeof(NexusMail.Application.Features.Email.Abstractions.IEmailAccountRepository)).ToList();
                foreach (var d in descriptors1) services.Remove(d);
                
                services.AddSingleton<NexusMail.Application.Features.Email.Abstractions.IEmailAccountRepository, FakeEmailAccountRepository>();

                var descriptors2 = services.Where(d => d.ServiceType == typeof(NexusMail.Application.Abstractions.Authentication.IWorkspaceContext)).ToList();
                foreach (var d in descriptors2) services.Remove(d);
                services.AddScoped<NexusMail.Application.Abstractions.Authentication.IWorkspaceContext, TestWorkspaceContext>();

                services.AddMassTransitTestHarness(x =>
                {
                    x.AddConsumer<NexusMail.Worker.AI.Consumers.SummaryRequestedConsumer>();
                    x.AddConsumer<NexusMail.Worker.AI.Consumers.PriorityRequestedConsumer>();
                    x.AddConsumer<NexusMail.Worker.AI.Consumers.ClassificationRequestedConsumer>();
                    x.AddConsumer<NexusMail.Worker.AI.Consumers.EmbeddingRequestedConsumer>();
                    
                    x.AddConsumer<NexusMail.Worker.Automation.Consumers.EvaluateRulesConsumer>();
                    x.AddConsumer<NexusMail.Worker.Automation.Consumers.ActionExecutionConsumer>();
                });

                services.AddSingleton<NexusMail.Automation.RuleEngine.IRuleEvaluator, NexusMail.Automation.RuleEngine.RuleEvaluator>();
                services.AddScoped<NexusMail.Automation.RuleEngine.IRuleEvaluationService, NexusMail.Worker.Automation.Services.RuleEvaluationService>();
                
                services.AddScoped<NexusMail.Automation.Actions.IActionExecutor, NexusMail.Automation.Actions.LabelActionExecutor>();
                services.AddScoped<NexusMail.Automation.Actions.IActionExecutor, NexusMail.Automation.Actions.AutoReplyActionExecutor>();
                services.AddScoped<NexusMail.Automation.Actions.IActionExecutor, NexusMail.Automation.Actions.NotifyActionExecutor>();

                // Mock IEmailProvider for ForwardActionExecutor
                var mockEmailProvider = new Moq.Mock<NexusMail.Application.Abstractions.Email.IEmailProvider>();
                mockEmailProvider
                    .Setup(x => x.SendEmailAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), default))
                    .Returns(System.Threading.Tasks.Task.CompletedTask);
                services.AddSingleton<NexusMail.Application.Abstractions.Email.IEmailProvider>(mockEmailProvider.Object);
                
                // Mock IWorkspaceRepository so WorkspaceContextEndpointFilter passes
                var workspaceRepoDescriptor = services.Where(d => d.ServiceType == typeof(NexusMail.Domain.Workspace.Repositories.IWorkspaceRepository)).ToList();
                foreach (var d in workspaceRepoDescriptor) services.Remove(d);
                var mockWorkspaceRepo = new Moq.Mock<NexusMail.Domain.Workspace.Repositories.IWorkspaceRepository>();
                mockWorkspaceRepo
                    .Setup(x => x.GetByIdAsync(Moq.It.IsAny<System.Guid>(), Moq.It.IsAny<System.Threading.CancellationToken>()))
                    .Returns(System.Threading.Tasks.Task.FromResult<NexusMail.Domain.Workspace.Entities.Workspace?>(NexusMail.Domain.Workspace.Entities.Workspace.Create("Test", System.Guid.NewGuid())));
                mockWorkspaceRepo
                    .Setup(x => x.GetRoleAsync(Moq.It.IsAny<System.Guid>(), Moq.It.IsAny<System.Guid>(), Moq.It.IsAny<System.Threading.CancellationToken>()))
                    .Returns(System.Threading.Tasks.Task.FromResult<NexusMail.Domain.Workspace.Enums.WorkspaceRole?>(NexusMail.Domain.Workspace.Enums.WorkspaceRole.Owner));
                services.AddScoped<NexusMail.Domain.Workspace.Repositories.IWorkspaceRepository>(_ => mockWorkspaceRepo.Object);

                services.AddScoped<NexusMail.Automation.Actions.IActionExecutor, NexusMail.Automation.Actions.ForwardActionExecutor>();
                services.AddScoped<NexusMail.Automation.Actions.ActionExecutorFactory>();

                // Configure PostgreSQL Database
                var options = services.Where(r => r.ServiceType.Name.Contains("DbContextOptions")).ToList();
                foreach(var option in options)
                {
                    services.Remove(option);
                }

                services.AddDbContext<ApplicationDbContext>((sp, opts) =>
                {
                    var interceptor = sp.GetRequiredService<NexusMail.Infrastructure.Persistence.Interceptors.ConvertDomainEventsToOutboxMessagesInterceptor>();
                    opts.UseNpgsql(_dbContainer.GetConnectionString())
                        .AddInterceptors(interceptor);
                });

                services.AddDbContext<NexusMail.Infrastructure.Search.Persistence.SearchDbContext>(opts =>
                {
                    opts.UseInMemoryDatabase("TestSearchDb");
                });
            });
        }
    }

    public class TestWorkspaceContext : NexusMail.Application.Abstractions.Authentication.IWorkspaceContext
    {
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;
        public TestWorkspaceContext(Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public System.Guid? WorkspaceId
        {
            get
            {
                var headerValue = _httpContextAccessor.HttpContext?.Request?.Headers["X-Workspace-Id"].ToString();
                if (System.Guid.TryParse(headerValue, out var workspaceId))
                {
                    return workspaceId;
                }
                
                return System.Guid.Parse("11111111-1111-1111-1111-111111111111");
            }
        }
    }

    public class FakeEmailAccountRepository : NexusMail.Application.Features.Email.Abstractions.IEmailAccountRepository
    {
        public System.Threading.Tasks.Task<bool> ExistsAsync(System.Guid workspaceId, string emailAddress, System.Threading.CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(false);
        }
        
        public System.Threading.Tasks.Task AddAsync(NexusMail.Domain.Email.Entities.EmailAccount emailAccount, System.Threading.CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task UpdateAsync(NexusMail.Domain.Email.Entities.EmailAccount emailAccount, System.Threading.CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task<NexusMail.Domain.Email.Entities.EmailAccount?> GetByIdAsync(System.Guid id, System.Threading.CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult<NexusMail.Domain.Email.Entities.EmailAccount?>(null);
        }

        public System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<NexusMail.Domain.Email.Entities.EmailAccount>> GetByWorkspaceIdAsync(System.Guid workspaceId, System.Threading.CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult<System.Collections.Generic.IEnumerable<NexusMail.Domain.Email.Entities.EmailAccount>>(new System.Collections.Generic.List<NexusMail.Domain.Email.Entities.EmailAccount>());
        }

        public System.Threading.Tasks.Task<NexusMail.Domain.Email.Entities.EmailAccount?> GetByProviderIdAsync(string providerId, System.Threading.CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult<NexusMail.Domain.Email.Entities.EmailAccount?>(null);
        }

        public System.Threading.Tasks.Task<(System.Collections.Generic.List<NexusMail.Domain.Email.Entities.EmailAccount> Items, int TotalCount)> GetPagedByWorkspaceIdAsync(System.Guid workspaceId, int page, int pageSize, System.Threading.CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult((new System.Collections.Generic.List<NexusMail.Domain.Email.Entities.EmailAccount>(), 0));
        }

        public System.Threading.Tasks.Task DeleteAsync(NexusMail.Domain.Email.Entities.EmailAccount emailAccount, System.Threading.CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task<System.Collections.Generic.List<NexusMail.Domain.Email.Entities.EmailAccount>> GetAccountsDueForSyncAsync(int batchSize, System.Threading.CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(new System.Collections.Generic.List<NexusMail.Domain.Email.Entities.EmailAccount>());
        }
    }
}
