using NexusMail.Worker.Notification;

using NexusMail.Infrastructure.Observability;

var builder = Host.CreateApplicationBuilder(args);

builder.UseNexusMailLogging();
builder.AddNexusMailObservability("NexusMail.Worker.Notification");
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
