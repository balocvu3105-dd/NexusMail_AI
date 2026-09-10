using System;

namespace NexusMail.API.Models;

public record ApiResponse<T>(
    T Data,
    string TraceId,
    DateTimeOffset Timestamp
);
