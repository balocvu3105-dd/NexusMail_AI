using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Domain.Automation.Entities;

namespace NexusMail.Application.Features.Automation.Abstractions;

public interface IAutomationExecutionRepository
{
    Task<List<AutomationExecution>> GetExecutionsByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<AutomationExecution?> GetExecutionByIdAsync(Guid executionId, Guid workspaceId, CancellationToken cancellationToken = default);
    Task<List<AutomationActionExecution>> GetActionExecutionsAsync(Guid executionId, CancellationToken cancellationToken = default);
    Task<List<AutomationAudit>> GetAuditsByExecutionAsync(Guid executionId, CancellationToken cancellationToken = default);
}
