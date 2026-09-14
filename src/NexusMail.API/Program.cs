using MassTransit;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NexusMail.API.Endpoints.AI;
using NexusMail.API.Endpoints.Automation;
using NexusMail.API.Endpoints.Email;
using NexusMail.API.Endpoints.Identity;
using NexusMail.API.Endpoints.Workspace;
using NexusMail.API.Endpoints.Operations;
using NexusMail.API.Endpoints.Notification;
using NexusMail.API.Hubs;
using NexusMail.API.Middleware;
using NexusMail.Application;
using NexusMail.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using NexusMail.Search;
using Serilog;

using NexusMail.Infrastructure.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNexusMailLogging();

builder.AddNexusMailObservability("NexusMail.API");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSearchInfrastructure();
// Register EventHandlers assembly in MediatR (so Outbox BackgroundService can dispatch domain events to them)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(NexusMail.EventHandlers.Email.EmailReceivedDomainEventHandler).Assembly));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<NexusMail.API.Consumers.EmailReceivedNotificationConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitConnectionString = builder.Configuration.GetConnectionString("RabbitMq") ?? "amqp://guest:guest@localhost:5672";
        cfg.Host(new Uri(rabbitConnectionString));
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

var rateLimitOptions = new NexusMail.API.Configuration.RateLimitOptions();
builder.Configuration.GetSection(NexusMail.API.Configuration.RateLimitOptions.SectionName).Bind(rateLimitOptions);

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("LoginPolicy", opt =>
    {
        opt.PermitLimit = rateLimitOptions.LoginLimit;
        opt.Window = TimeSpan.FromSeconds(rateLimitOptions.LoginWindowSeconds);
    });

    options.AddFixedWindowLimiter("RefreshPolicy", opt =>
    {
        opt.PermitLimit = rateLimitOptions.RefreshLimit;
        opt.Window = TimeSpan.FromSeconds(rateLimitOptions.RefreshWindowSeconds);
    });
});

builder.Services.AddSignalR();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<NexusMail.Infrastructure.Persistence.ApplicationDbContext>("Database")
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "PostgreSQL")
    .AddRabbitMQ(rabbitConnectionString: builder.Configuration.GetConnectionString("RabbitMq") ?? "amqp://guest:guest@localhost:5672", name: "RabbitMQ")
    .AddDiskStorageHealthCheck(
        s => s.AddDrive(Path.GetPathRoot(Environment.SystemDirectory) ?? "/", 1024),
        name: "DiskSpace") // At least 1MB free
    .AddProcessAllocatedMemoryHealthCheck(1024, "Memory") // 1GB max
    .AddCheck<NexusMail.Infrastructure.Observability.PgVectorHealthCheck>("PgVector");

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection(NexusMail.Infrastructure.Identity.JwtSettings.SectionName).Get<NexusMail.Infrastructure.Identity.JwtSettings>();
        
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings?.Issuer,
            ValidAudience = jwtSettings?.Audience,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSettings?.Secret ?? string.Empty))
        };
        
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && 
                    (path.StartsWithSegments("/hub/notifications") || path.StartsWithSegments("/api/v1/auth/google/login")))
                {
                    context.Token = accessToken;
                }
                return System.Threading.Tasks.Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

var versionedGroup = app.MapGroup("").WithApiVersionSet(apiVersionSet);

versionedGroup.MapIdentityEndpoints();
versionedGroup.MapCreateWorkspaceEndpoint(); // Vertical slice
versionedGroup.MapGetWorkspaceEndpoints();
versionedGroup.MapEmailEndpoints();
versionedGroup.MapAIEndpoints();
versionedGroup.MapAutomationEndpoints();
versionedGroup.MapOperationsEndpoints();
versionedGroup.MapNotificationEndpoints();
versionedGroup.MapOAuthEndpoints();

app.MapHub<NotificationHub>("/hub/notifications");

app.Run();

public partial class Program { }
namespace NexusMail.API
{
    public interface IApiMarker { }
    public class ApiMarker { }
}
