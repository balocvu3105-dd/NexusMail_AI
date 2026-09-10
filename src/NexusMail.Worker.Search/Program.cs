using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NexusMail.Infrastructure.Persistence;
using NexusMail.Infrastructure.Search.Persistence;
using NexusMail.Worker.Search.Consumers;
using Pgvector.EntityFrameworkCore; // Added for UseVector

using NexusMail.Infrastructure.Observability;

var builder = Host.CreateApplicationBuilder(args);

builder.UseNexusMailLogging();
builder.AddNexusMailObservability("NexusMail.Worker.Search");

// DbContexts
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<SearchDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.UseVector()));

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AIProcessingCompletedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        cfg.Host(rabbitMqHost, "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        cfg.ReceiveEndpoint("search-index-queue", e =>
        {
            // Optional: concurrency limit
            e.PrefetchCount = 16;
            
            e.ConfigureConsumer<AIProcessingCompletedConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
