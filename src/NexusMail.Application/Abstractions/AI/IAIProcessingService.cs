using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Abstractions.AI;

public record EmailAnalysisResult(
    string Summary,
    int PriorityScore,
    string PriorityReason,
    string Category,
    double Confidence,
    IEnumerable<string> Tags,
    bool NeedsAttention);

public interface IAIProcessingService
{
    Task<Result<EmailAnalysisResult>> ProcessEmailAsync(string subject, string content, string sender, CancellationToken cancellationToken);
}
