using System;
using NexusMail.Shared.Common;
using NexusMail.Application.Abstractions.Messaging;
using NexusMail.Domain.Automation.Enums;

namespace NexusMail.Application.Features.Automation.Commands.CreateAutomationRule;

public record CreateAutomationRuleCommand(
    Guid WorkspaceId,
    string Name,
    TriggerType TriggerType,
    string ConditionsJson,
    string ActionsJson,
    string? Description = null,
    bool DryRun = false
) : ICommand<Result<Guid>>;
