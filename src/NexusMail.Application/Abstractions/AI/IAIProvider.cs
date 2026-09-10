using System.Threading;
using System.Threading.Tasks;

namespace NexusMail.Application.Abstractions.AI;

public interface IAIProvider
{
    Task<string> GenerateSummaryAsync(string content, CancellationToken cancellationToken = default);
}
