using MassTransit;
using NexusMail.Application;
using NexusMail.Infrastructure;
using NexusMail.Worker.AI.Consumers;
using NexusMail.Infrastructure.Observability;

var builder = Host.CreateApplicationBuilder(args);

builder.UseNexusMailLogging();
builder.AddNexusMailObservability("NexusMail.Worker.AI");

// Register Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Register MassTransit with Consumers
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SummaryRequestedConsumer>();
    x.AddConsumer<PriorityRequestedConsumer>();
    x.AddConsumer<ClassificationRequestedConsumer>();
    x.AddConsumer<EmbeddingRequestedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMq") ?? "rabbitmq://localhost", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });

        // Add UseMessageRetry for Transient AI Exceptions ONLY
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
