using Quartz;
using MassTransit;
using NexusMail.Application;
using NexusMail.Infrastructure;
using NexusMail.Worker.AI.Consumers;
using NexusMail.Worker.AI.Services;
using NexusMail.Infrastructure.Observability;

var builder = Host.CreateApplicationBuilder(args);

builder.UseNexusMailLogging();
builder.AddNexusMailObservability("NexusMail.Worker.AI");

// Register Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(NexusMail.EventHandlers.Email.EmailReceivedDomainEventHandler).Assembly));

// Add Quartz for MassTransit Delayed Redelivery
builder.Services.AddQuartz();

// Add Stale Recovery Sweeper for Crash Resilience
builder.Services.AddHostedService<StaleProcessingRecoveryService>();

// Register MassTransit with Consumers
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AIProcessingRequestedConsumer>();
    x.AddConsumer<EmbeddingRequestedConsumer>();
    x.AddConsumer<AIFaultConsumer>();

    // Add Quartz Consumers for Message Scheduling
    x.AddQuartzConsumers();
    x.AddMessageScheduler(new Uri("queue:scheduler"));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMq") ?? "rabbitmq://localhost", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });

        // Configure RabbitMQ to use the scheduler
        cfg.UseMessageScheduler(new Uri("queue:scheduler"));

        // Receive endpoint for the scheduler
        cfg.ReceiveEndpoint("scheduler", e =>
        {
            e.ConfigureQuartzConsumers(context);
        });

        // 1. Delayed Redelivery: if immediate retries exhaust, redeliver after 5 mins, then 15 mins
        cfg.UseDelayedRedelivery(r =>
        {
            r.Handle<NexusMail.Application.Abstractions.AI.AIProviderTransientException>();
            r.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15));
        });

        // 2. Immediate Retry: Try 3 times exponentially before giving up to delayed redelivery
        cfg.UseMessageRetry(r => 
        {
            r.Handle<NexusMail.Application.Abstractions.AI.AIProviderTransientException>();
            r.Exponential(3, TimeSpan.FromSeconds(2), TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(5));
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
