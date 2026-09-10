using System;
using NexusMail.Shared.Common;
using NexusMail.Application.Abstractions.Messaging;
using NexusMail.Domain.Automation.Enums;

namespace NexusMail.Application.Features.Automation.Commands.UpdateAutomationRule;

public record UpdateAutomationRuleCommand(
    Guid RuleId,
    Guid WorkspaceId,
    string Name,
    string ConditionsJson,
    string ActionsJson,
    string? Description = null
) : ICommand<Result>;
