using System.Threading;
using System.Threading.Tasks;

namespace NexusMail.Application.Abstractions.Email;

public interface IEmailProvider
{
    /// <summary>
    /// Sends an email through the external provider.
    /// NexusMail guarantees idempotent invocation at the provider boundary.
    /// If an idempotencyKey is provided, the same key + same logical request 
    /// MUST NOT produce another side effect if the provider implementation supports it.
    /// </summary>
    Task SendEmailAsync(string to, string subject, string body, string? idempotencyKey = null, CancellationToken cancellationToken = default);
}
