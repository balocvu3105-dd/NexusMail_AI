using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace NexusMail.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {Name} {@Request}", requestName, request);
        var response = await next();
        _logger.LogInformation("Handled {Name} {@Response}", requestName, response);
        return response;
    }
}
