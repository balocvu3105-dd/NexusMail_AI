using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace NexusMail.Infrastructure.Observability;

public static class LoggingExtensions
{
    public static IHostBuilder UseNexusMailLogging(this IHostBuilder builder)
    {
        return builder.UseSerilog((context, services, configuration) =>
        {
            ConfigureSerilog(configuration, context.Configuration, services);
        });
    }

    public static IHostApplicationBuilder UseNexusMailLogging(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSerilog((services, configuration) =>
        {
            ConfigureSerilog(configuration, builder.Configuration, services);
        });
        return builder;
    }

    private static void ConfigureSerilog(LoggerConfiguration configuration, Microsoft.Extensions.Configuration.IConfiguration appConfiguration, IServiceProvider services)
    {
        configuration
            .ReadFrom.Configuration(appConfiguration)
            .ReadFrom.Services(services)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "NexusMail")
            // .Enrich.WithMachineName()
            // .Enrich.WithEnvironmentName()
            .WriteTo.Console(new CompactJsonFormatter());
    }
}
