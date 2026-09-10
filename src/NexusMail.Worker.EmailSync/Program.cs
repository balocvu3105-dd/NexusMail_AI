using NexusMail.Worker.EmailSync;
using NexusMail.Application;
using NexusMail.Infrastructure;
using MassTransit;

using NexusMail.Infrastructure.Observability;

var builder = Host.CreateApplicationBuilder(args);

builder.UseNexusMailLogging();
builder.AddNexusMailObservability("NexusMail.Worker.EmailSync");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMq"));
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
