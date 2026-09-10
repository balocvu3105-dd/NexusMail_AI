using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace NexusMail.Application.Behaviors;

public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly Stopwatch _timer = new();
    private readonly ILogger<TRequest> _logger;

    public PerformanceBehavior(ILogger<TRequest> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();
        var response = await next();
        _timer.Stop();

        if (_timer.ElapsedMilliseconds > 500)
        {
            _logger.LogWarning("Long Running Request: {Name} ({ElapsedMilliseconds} ms) {@Request}", typeof(TRequest).Name, _timer.ElapsedMilliseconds, request);
        }

        return response;
    }
}
