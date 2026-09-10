using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Npgsql;
using System;

namespace NexusMail.Infrastructure.Observability;

public static class ObservabilityExtensions
{
    public static IHostApplicationBuilder AddNexusMailObservability(this IHostApplicationBuilder builder, string serviceName)
    {
        var otlpEndpoint = builder.Configuration["OpenTelemetry:Endpoint"];
        bool useOtlp = !string.IsNullOrWhiteSpace(otlpEndpoint);

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName)
                .AddTelemetrySdk()
                .AddEnvironmentVariableDetector())
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(CustomMetrics.ActivitySource.Name)
                    .AddSource("MassTransit")
                    .SetSampler(new AlwaysOnSampler())
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                    })
                    .AddNpgsql();

                if (useOtlp)
                {
                    tracing.AddOtlpExporter(opts => 
                    {
                        opts.Endpoint = new Uri(otlpEndpoint!);
                    });
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(CustomMetrics.MeterName)
                    .AddMeter("MassTransit")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
                    //.AddProcessInstrumentation();

                metrics.AddPrometheusExporter();
                
                if (useOtlp)
                {
                    metrics.AddOtlpExporter(opts => 
                    {
                        opts.Endpoint = new Uri(otlpEndpoint!);
                    });
                }
            });

        builder.Services.AddHostedService<OutboxMetricsBackgroundService>();

        return builder;
    }
}
