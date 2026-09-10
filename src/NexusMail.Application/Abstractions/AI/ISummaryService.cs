using System.Threading;
using System.Threading.Tasks;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Abstractions.AI;

public interface ISummaryService
{
    Task<Result<string>> GenerateSummaryAsync(string subject, string body, CancellationToken cancellationToken = default);
}
