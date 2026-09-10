using System;
using System.Threading;

namespace NexusMail.Application.Abstractions.Context;

public interface IExecutionContextAccessor
{
    IExecutionContext Context { get; }
    void SetContext(IExecutionContext context);
}

public class ExecutionContext : IExecutionContext
{
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString("N");
    public Guid? WorkspaceId { get; init; }
    public Guid? UserId { get; init; }
    public string? TraceId { get; init; }
    public string? RequestId { get; init; }
    public string? TenantId { get; init; }
}

public class ExecutionContextAccessor : IExecutionContextAccessor
{
    private static readonly AsyncLocal<IExecutionContext> _executionContextCurrent = new();

    public IExecutionContext Context
    {
        get
        {
            if (_executionContextCurrent.Value == null)
            {
                _executionContextCurrent.Value = new ExecutionContext();
            }
            return _executionContextCurrent.Value;
        }
    }

    public void SetContext(IExecutionContext context)
    {
        _executionContextCurrent.Value = context;
    }
}
