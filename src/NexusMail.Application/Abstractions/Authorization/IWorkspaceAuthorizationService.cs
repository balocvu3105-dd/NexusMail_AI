using System;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Abstractions.Authorization;

public interface IWorkspaceAuthorizationService
{
    Task<Result> CanInviteAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
    Task<Result> CanRemoveMemberAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
    Task<Result> CanManageBillingAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
    Task<Result> CanCreateAutomationAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
    Task<Result> CanDeleteWorkspaceAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
}
