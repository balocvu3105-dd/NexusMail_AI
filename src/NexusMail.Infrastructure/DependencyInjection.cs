using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Application.Abstractions.Messaging;
using NexusMail.Application.Abstractions.Outbox;
using NexusMail.Application.Abstractions.Persistence;
using NexusMail.Application.Abstractions.Time;
using NexusMail.Application.Abstractions.AI;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Application.Features.Email.Pipeline;
using NexusMail.Application.Features.Email.Pipeline.Steps;
using NexusMail.Domain.Email.Enums;
using NexusMail.Domain.Identity.Repositories;
using NexusMail.Domain.Workspace.Repositories;
using NexusMail.Notification.Domain;
using NexusMail.Infrastructure.AI.Providers;
using NexusMail.Infrastructure.Email.Factory;
using NexusMail.Infrastructure.Email.Providers.Google;
using NexusMail.Infrastructure.Email.Providers.Outlook;
using NexusMail.Infrastructure.Email.Providers.Imap;
using NexusMail.Infrastructure.Messaging;
using NexusMail.Infrastructure.Outbox;
using NexusMail.Infrastructure.Persistence;
using NexusMail.Infrastructure.Persistence.Interceptors;
using NexusMail.Infrastructure.Persistence.Repositories;
using NexusMail.Infrastructure.Services;
using System;

namespace NexusMail.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ─── Outbox ────────────────────────────────────────────────────────
        services.AddOptions<OutboxOptions>()
                .BindConfiguration(OutboxOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

        services.AddSingleton<IOutboxSerializer, NexusMail.Infrastructure.Outbox.SystemTextJsonOutboxSerializer>();
        services.AddSingleton<ConvertDomainEventsToOutboxMessagesInterceptor>();

        // ─── Database ──────────────────────────────────────────────────────
        var connectionString = configuration.GetConnectionString("Database") ??
            "Host=localhost;Database=nexusmail;Username=postgres;Password=password";

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>();
            options.UseNpgsql(connectionString).AddInterceptors(interceptor);
        });

        services.AddDbContext<NexusMail.Infrastructure.Search.Persistence.SearchDbContext>(options =>
        {
            options.UseNpgsql(connectionString, o => o.UseVector());
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // ─── Repositories ──────────────────────────────────────────────────
        services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IEmailAccountRepository, EmailAccountRepository>();
        services.AddScoped<IEmailRepository, EmailRepository>();
        services.AddScoped<NexusMail.Application.Features.Automation.Abstractions.IAutomationRuleRepository, AutomationRuleRepository>();
        services.AddScoped<NexusMail.Application.Features.Automation.Abstractions.IAutomationExecutionRepository, AutomationExecutionRepository>();
        services.AddScoped<NexusMail.Application.Features.Automation.Abstractions.IAutomationAnalyticsRepository, AutomationAnalyticsRepository>();
        services.AddScoped<INotificationRepository, NexusMail.Infrastructure.Persistence.Repositories.Notification.NotificationRepository>();

        // ─── Outbox Dispatcher ─────────────────────────────────────────────
        services.AddScoped<IOutboxDispatcher, NexusMail.Infrastructure.Outbox.OutboxDispatcher>();
        services.AddHostedService<NexusMail.Infrastructure.Outbox.OutboxBackgroundService>();

        // ─── Email Providers (Plugin Architecture — Keyed DI) ──────────────
        // Each provider is registered with a key = EmailProvider enum value.
        // EmailProviderFactory resolves the correct provider by key.
        services.AddKeyedScoped<IEmailProvider, GoogleEmailProvider>(EmailProvider.Google);
        services.AddKeyedScoped<IEmailProvider, OutlookEmailProvider>(EmailProvider.Outlook);
        services.AddKeyedScoped<IEmailProvider, OutlookEmailProvider>(EmailProvider.Microsoft365);
        services.AddKeyedScoped<IEmailProvider, ImapEmailProvider>(EmailProvider.IMAP);
        services.AddKeyedScoped<IEmailProvider, NexusMail.Infrastructure.Email.Providers.Fake.FakeEmailProvider>(EmailProvider.Fake);

        services.AddHttpClient<GoogleEmailProvider>();

        services.AddScoped<IEmailProviderFactory, EmailProviderFactory>();

        // ─── Email Sync Support ────────────────────────────────────────────
        services.AddScoped<ISynchronizationMetrics, NexusMail.Infrastructure.Email.LoggingSynchronizationMetrics>();

        var encryptionKey = configuration["Security:TokenEncryptionKey"] ?? "12345678901234567890123456789012";
        services.AddSingleton<ITokenEncryptionService>(new TokenEncryptionService(encryptionKey));

        services.AddOptions<NexusMail.Infrastructure.Authentication.GoogleOAuthOptions>()
            .BindConfiguration(NexusMail.Infrastructure.Authentication.GoogleOAuthOptions.SectionName);

        services.AddScoped<IOAuthProvider, NexusMail.Infrastructure.Authentication.GoogleOAuthProvider>();
        services.AddScoped<IOAuthProvider, NexusMail.Infrastructure.Authentication.FakeOAuthProvider>();

        // ─── Email Sync Pipeline Steps (registered in execution order) ─────
        // NOTE: EmailSyncPipeline receives IEnumerable<ISyncPipelineStep> — order matters.
        services.AddScoped<ISyncPipelineStep, FetchEmailsStep>();
        services.AddScoped<ISyncPipelineStep, RetryStep>();
        services.AddScoped<ISyncPipelineStep, RateLimitStep>();
        services.AddScoped<ISyncPipelineStep, ValidateEmailsStep>();
        services.AddScoped<ISyncPipelineStep, NormalizeEmailsStep>();
        services.AddScoped<ISyncPipelineStep, DeduplicateEmailsStep>();
        services.AddScoped<ISyncPipelineStep, PersistEmailsStep>();
        services.AddScoped<ISyncPipelineStep, PublishDomainEventsStep>();
        services.AddScoped<ISyncPipelineStep, RecordMetricsStep>();
        services.AddScoped<EmailSyncPipeline>();

        // ─── Email Sync Service ────────────────────────────────────────────
        services.AddScoped<IEmailSynchronizationService,
            NexusMail.Application.Features.Email.Services.EmailSynchronizationService>();

        // Token service — orchestrates token lifecycle
        services.AddScoped<IEmailTokenService, NexusMail.Infrastructure.Authentication.EmailTokenService>();

        services.AddScoped<IEmailMapper>(sp => new StubEmailMapper());
        services.AddScoped<IEmailPersistenceService, EmailPersistenceService>();
        services.AddScoped<IInboxQueryService, NexusMail.Infrastructure.Email.InboxQueryService>();
        services.AddScoped<NexusMail.Application.Features.Email.Abstractions.IInboxStatusQueryService, NexusMail.Infrastructure.Email.InboxStatusQueryService>();

        // ─── AI Providers (Keyed — resolve by name) ────────────────────────
        // Sprint 5: activate the provider matching configuration["AI:DefaultProvider"]
        services.AddScoped<IAIModelProvider, OpenAIModelProvider>();   // default registration
        services.AddKeyedScoped<IAIModelProvider, OpenAIModelProvider>("openai");
        services.AddKeyedScoped<IAIModelProvider, ClaudeModelProvider>("claude");
        services.AddKeyedScoped<IAIModelProvider, GeminiModelProvider>("gemini");
        services.AddKeyedScoped<IAIModelProvider, OllamaModelProvider>("ollama");

        services.AddScoped<IAIModelProviderFactory, NexusMail.Infrastructure.AI.Factory.AIModelProviderFactory>();

        services.AddScoped<NexusMail.Infrastructure.AI.Services.OpenAILanguageService>();
        services.AddScoped<IAIProcessingService>(sp => sp.GetRequiredService<NexusMail.Infrastructure.AI.Services.OpenAILanguageService>());
        services.AddScoped<IEmbeddingService>(sp => sp.GetRequiredService<NexusMail.Infrastructure.AI.Services.OpenAILanguageService>());
        services.AddScoped<NexusMail.Application.Abstractions.Copilot.ICopilotLanguageService, NexusMail.Infrastructure.AI.Services.CopilotLanguageService>();

        // Dummy Kernel to fix "No service for type 'Microsoft.SemanticKernel.Kernel' has been registered"
        services.AddScoped<Microsoft.SemanticKernel.Kernel>(sp => null!);

        // ─── Copilot Actions ───────────────────────────────────────────────
        services.AddMemoryCache(); // For IdempotencyCache
        services.AddScoped<NexusMail.Application.Features.Copilot.ICopilotActionService, NexusMail.Infrastructure.AI.Services.CopilotActionService>();
        
        // Register ActionExecutors (for Copilot to use via Factory)
        services.AddScoped<NexusMail.Automation.Actions.IActionExecutor, NexusMail.Automation.Actions.LabelActionExecutor>();
        services.AddScoped<NexusMail.Automation.Actions.IActionExecutor, NexusMail.Automation.Actions.CreateTaskActionExecutor>();
        // Only Tier 1 registered for Copilot in API layer
        services.AddScoped<NexusMail.Automation.Actions.ActionExecutorFactory>();

        // ─── Identity ──────────────────────────────────────────────────────
        services.AddOptions<NexusMail.Infrastructure.Identity.JwtSettings>()
            .BindConfiguration(NexusMail.Infrastructure.Identity.JwtSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IPasswordHasher, NexusMail.Infrastructure.Identity.PasswordHasher>();
        services.AddScoped<ITokenService, NexusMail.Infrastructure.Identity.JwtTokenService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, NexusMail.Infrastructure.Authentication.CurrentUser>();
        services.AddScoped<IWorkspaceContext, NexusMail.Infrastructure.Authentication.WorkspaceContext>();

        // ─── Messaging ────────────────────────────────────────────────────
        services.AddTransient<IClock, SystemClock>();
        services.AddScoped<IMessageBus, MessageBus>();

        // ─── Search ────────────────────────────────────────────────────────
        services.AddScoped<NexusMail.Application.Abstractions.Search.ISearchEngine, StubSearchEngine>();

        // Default EmailProvider to resolve non-keyed DI (e.g. ForwardActionExecutor)
        services.AddScoped<IEmailProvider, StubEmailProvider>();

        return services;
    }
}

// ─── Stubs (to be replaced in Sprint 4) ─────────────────────────────────────
// These stubs exist only to allow compilation while real implementations are built.
// They are named "Stub" (not "Mock") to make clear they are WIP, not test doubles.

internal sealed class StubSearchEngine : NexusMail.Application.Abstractions.Search.ISearchEngine
{
    public Task<NexusMail.Application.Abstractions.Search.SearchResult> SearchAsync(NexusMail.Application.Abstractions.Search.SearchQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(new NexusMail.Application.Abstractions.Search.SearchResult());

    public Task<NexusMail.Application.Abstractions.Search.SearchResult> FindSimilarAsync(Guid emailId, Guid workspaceId, int limit = 5, CancellationToken cancellationToken = default)
        => Task.FromResult(new NexusMail.Application.Abstractions.Search.SearchResult());
}

internal sealed class StubEmailProvider : IEmailProvider
{
    public Task<ProviderProfile> GetProfileAsync(string accessToken, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
    
    public Task<IReadOnlyList<ProviderLabel>> GetLabelsAsync(string accessToken, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
    
    public Task<ProviderMessagePage> GetMessagesAsync(string accessToken, string? pageToken = null, string? cursor = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
    
    public Task<ProviderMessage> GetMessageAsync(string accessToken, string messageId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
    
    public Task<ProviderTokens> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
        
    public Task<bool> SendEmailAsync(string to, string subject, string body, string? idempotencyKey = null, CancellationToken cancellationToken = default)
        => Task.FromResult(true);
}

internal sealed class StubEmailMapper : IEmailMapper
{
    public object MapToDomainMessage(ProviderMessage providerMessage, Guid emailAccountId)
    {
        return new NexusMail.Domain.Email.Entities.Email(
            emailAccountId,
            Guid.Empty, // WorkspaceId (stub)
            providerMessage.Id,
            "sender@unknown.com", // Extract from headers if available
            providerMessage.Subject ?? "No Subject",
            providerMessage.Body ?? providerMessage.Snippet,
            System.DateTimeOffset.UtcNow
        );
    }
}

internal sealed class EmailPersistenceService : IEmailPersistenceService
{
    private readonly NexusMail.Infrastructure.Persistence.ApplicationDbContext _dbContext;

    public EmailPersistenceService(NexusMail.Infrastructure.Persistence.ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveEmailsAsync(IReadOnlyList<object> domainMessages, CancellationToken cancellationToken = default)
    {
        var emails = domainMessages.Cast<NexusMail.Domain.Email.Entities.Email>().ToList();
        if (emails.Any())
        {
            await _dbContext.Emails.AddRangeAsync(emails, cancellationToken);
        }
    }

    public async Task<HashSet<string>> GetExistingMessageIdsAsync(System.Guid emailAccountId, IEnumerable<string> messageIds, CancellationToken cancellationToken = default)
    {
        var ids = messageIds.ToList();
        if (ids.Count == 0) return new HashSet<string>();

        var existing = await _dbContext.Emails
            .Where(e => e.AccountId == emailAccountId && ids.Contains(e.MessageId))
            .Select(e => e.MessageId)
            .ToListAsync(cancellationToken);

        return existing.ToHashSet();
    }
}
