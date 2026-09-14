using System.Threading;
using System.Threading.Tasks;

namespace NexusMail.Application.Abstractions.Integration;

public record CreateTaskParameters(string Provider, string Title, string? Description, string? Assignee);

public interface ITaskManagementProvider
{
    Task<string> CreateTaskAsync(CreateTaskParameters parameters, string idempotencyKey, CancellationToken cancellationToken = default);
}
