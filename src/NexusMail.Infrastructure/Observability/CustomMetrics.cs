using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace NexusMail.Infrastructure.Observability;

public static class CustomMetrics
{
    public const string MeterName = "NexusMail.Platform";

    public static readonly Meter Meter = new(MeterName, "1.0.0");
    public static readonly ActivitySource ActivitySource = new(MeterName, "1.0.0");

    public static readonly Counter<long> EmailsProcessed = Meter.CreateCounter<long>(
        "emails_processed_total", "emails", "Total number of emails synced and processed");

    public static readonly Counter<long> AiRequests = Meter.CreateCounter<long>(
        "ai_requests_total", "requests", "Total number of AI requests made");

    public static readonly Counter<long> AutomationRulesExecuted = Meter.CreateCounter<long>(
        "automation_rules_executed_total", "executions", "Total number of automation rules executed");

    public static readonly Counter<long> SearchQueries = Meter.CreateCounter<long>(
        "search_queries_total", "queries", "Total number of search queries made");

    public static readonly Histogram<double> SearchLatency = Meter.CreateHistogram<double>(
        "search_latency_ms", "ms", "Latency of search queries");

    public static readonly Counter<double> AiCost = Meter.CreateCounter<double>(
        "ai_cost_usd_total", "usd", "Total estimated AI cost");

    // Worker Metrics
    public static readonly UpDownCounter<int> WorkerQueueLength = Meter.CreateUpDownCounter<int>(
        "worker_queue_length", "messages", "Number of messages in the worker queues");

    public static readonly Histogram<double> WorkerProcessingTime = Meter.CreateHistogram<double>(
        "worker_processing_time", "ms", "Time taken to process worker messages");

    public static readonly Counter<long> WorkerFailures = Meter.CreateCounter<long>(
        "worker_failures_total", "failures", "Total number of worker failures");

    public static readonly Histogram<double> EmailSyncDuration = Meter.CreateHistogram<double>(
        "email_sync_duration", "ms", "Duration of email synchronization");

    public static readonly Histogram<double> AiEmbeddingDuration = Meter.CreateHistogram<double>(
        "ai_embedding_duration", "ms", "Duration of AI embedding generation");

    public static readonly Histogram<double> AutomationExecutionDuration = Meter.CreateHistogram<double>(
        "automation_execution_duration", "ms", "Duration of automation rule execution");
}
