using System.Threading;
using System.Threading.Tasks;

namespace NexusMail.Application.Abstractions.Copilot;

public interface ICopilotOrchestrator
{
    Task<CopilotResponse> AskAsync(CopilotQuery query, CancellationToken cancellationToken = default);
    IAsyncEnumerable<CopilotStreamChunk> AskStreamAsync(CopilotQuery query, CancellationToken cancellationToken = default);
}
