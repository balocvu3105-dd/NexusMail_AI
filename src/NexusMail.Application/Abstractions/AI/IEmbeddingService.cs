using System.Threading;
using System.Threading.Tasks;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Abstractions.AI;

public interface IEmbeddingService
{
    Task<Result<float[]>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}
