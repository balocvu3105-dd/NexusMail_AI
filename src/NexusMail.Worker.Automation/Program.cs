using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NexusMail.Automation.Actions;
using NexusMail.Automation.RuleEngine;
using NexusMail.Infrastructure.Persistence;
using NexusMail.Worker.Automation.Consumers;

using NexusMail.Infrastructure.Observability;

var builder = Host.CreateApplicationBuilder(args);

builder.UseNexusMailLogging();
builder.AddNexusMailObservability("NexusMail.Worker.Automation");

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Rule Engine
builder.Services.AddSingleton<IRuleEvaluator, RuleEvaluator>();
builder.Services.AddScoped<IRuleEvaluationService, NexusMail.Worker.Automation.Services.RuleEvaluationService>();
builder.Services.AddScoped<NexusMail.Application.Abstractions.Context.IExecutionContextAccessor, NexusMail.Application.Abstractions.Context.ExecutionContextAccessor>();
builder.Services.AddScoped<NexusMail.Application.Abstractions.Email.IEmailProvider, WorkerAutomationStubEmailProvider>();

// Action Executors
builder.Services.AddScoped<IActionExecutor, LabelActionExecutor>();
builder.Services.AddScoped<IActionExecutor, AutoReplyActionExecutor>();
builder.Services.AddScoped<IActionExecutor, ForwardActionExecutor>();
builder.Services.AddScoped<IActionExecutor, NotifyActionExecutor>();
builder.Services.AddScoped<ActionExecutorFactory>();

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<EvaluateRulesConsumer>();
    x.AddConsumer<AIProcessingCompletedConsumer>();
    x.AddConsumer<ActionExecutionConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMq"));

        cfg.ReceiveEndpoint("automation-ai-completed", e =>
        {
            e.ConfigureConsumer<AIProcessingCompletedConsumer>(context);
        });

        cfg.ReceiveEndpoint("automation-evaluate-rules", e =>
        {
            e.ConfigureConsumer<EvaluateRulesConsumer>(context);
        });
        
        cfg.ReceiveEndpoint("automation-action-execution", e =>
        {
            // Polly Retry configuration here
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
            e.ConfigureConsumer<ActionExecutionConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
class WorkerAutomationStubEmailProvider : NexusMail.Application.Abstractions.Email.IEmailProvider { public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default) => Task.CompletedTask; }
