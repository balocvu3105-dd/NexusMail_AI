using System;

namespace NexusMail.Application.Features.Email.Abstractions;

public interface ISynchronizationMetrics
{
    // Counters
    void IncrementEmailsSynced(int count = 1);
    void IncrementRetryCount();
    void IncrementFailedSyncs();

    // Histograms
    void RecordSyncDuration(TimeSpan duration);

    // Gauges
    void IncrementRunningWorkers();
    void DecrementRunningWorkers();
}
