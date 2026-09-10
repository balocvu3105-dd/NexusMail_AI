using System;

namespace NexusMail.Application.Abstractions.Context;

public interface IExecutionContext
{
    string CorrelationId { get; }
    Guid? WorkspaceId { get; }
    Guid? UserId { get; }
    string? TraceId { get; }
    string? RequestId { get; }
    string? TenantId { get; }
}
