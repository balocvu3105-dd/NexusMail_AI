using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace NexusMail.Infrastructure.Observability;

public class OpenAIHealthCheck : IHealthCheck
{
    private readonly Kernel _kernel;
    private readonly ILogger<OpenAIHealthCheck> _logger;

    public OpenAIHealthCheck(Kernel kernel, ILogger<OpenAIHealthCheck> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple ping to OpenAI models API
            var result = await _kernel.InvokePromptAsync("Reply 'OK'", cancellationToken: cancellationToken);
            
            if (result != null && !string.IsNullOrWhiteSpace(result.GetValue<string>()))
            {
                return HealthCheckResult.Healthy("OpenAI connection successful");
            }
            
            return HealthCheckResult.Unhealthy("OpenAI returned empty response");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "OpenAI HealthCheck failed");
            return HealthCheckResult.Unhealthy("OpenAI connection failed", ex);
        }
    }
}
