using System;
using NexusMail.Shared.Common;
using NexusMail.Application.Abstractions.Messaging;
using NexusMail.Application.Features.Automation.Queries.GetAutomationRules;

namespace NexusMail.Application.Features.Automation.Queries.GetAutomationRuleById;

public record GetAutomationRuleByIdQuery(Guid RuleId, Guid WorkspaceId) : IQuery<Result<AutomationRuleDto>>;
