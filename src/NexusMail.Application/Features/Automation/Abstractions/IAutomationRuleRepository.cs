using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Domain.Automation.Entities;

namespace NexusMail.Application.Features.Automation.Abstractions;

public interface IAutomationRuleRepository
{
    Task<List<AutomationRule>> GetRulesByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<AutomationRule?> GetRuleByIdAsync(Guid ruleId, Guid workspaceId, CancellationToken cancellationToken = default);
    Task AddRuleAsync(AutomationRule rule, CancellationToken cancellationToken = default);
    Task UpdateRuleAsync(AutomationRule rule, CancellationToken cancellationToken = default);
}
