using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Abstractions.AI;

public interface IClassificationService
{
    Task<Result<AIClassificationResult>> GenerateClassificationAsync(string subject, string body, string sender, CancellationToken cancellationToken = default);
}

public record AIClassificationResult(string Category, double Confidence, IReadOnlyList<string> Tags);
