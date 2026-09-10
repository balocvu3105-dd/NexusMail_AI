using System.Threading;
using System.Threading.Tasks;

namespace NexusMail.Application.Abstractions.Email;

public interface IEmailProvider
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
