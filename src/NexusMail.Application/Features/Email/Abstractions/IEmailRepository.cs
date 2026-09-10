using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Domain.AI.Entities;

namespace NexusMail.Application.Features.Email.Abstractions;

public interface IEmailRepository
{
    Task<NexusMail.Domain.Email.Entities.Email?> GetByIdAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default);
    Task<(NexusMail.Domain.Email.Entities.Email Email, AIAnalysis? Analysis)?> GetEmailWithAnalysisAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default);
    Task<List<NexusMail.Domain.Email.Entities.Email>> GetByIdsAsync(IEnumerable<Guid> ids, Guid workspaceId, CancellationToken cancellationToken = default);
    Task AddAnalysisAsync(AIAnalysis analysis, CancellationToken cancellationToken = default);
    void UpdateAnalysis(AIAnalysis analysis);
}
