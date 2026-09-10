using System.Threading;
using System.Threading.Tasks;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Abstractions.AI;

public interface IPriorityService
{
    Task<Result<AIPriorityResult>> GeneratePriorityAsync(string subject, string body, string sender, CancellationToken cancellationToken = default);
}

public record AIPriorityResult(int Score, string Reason, bool RequiresAction);
