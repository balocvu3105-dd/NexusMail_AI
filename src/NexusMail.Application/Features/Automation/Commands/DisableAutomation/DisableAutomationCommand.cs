using System;
using NexusMail.Shared.Common;
using NexusMail.Application.Abstractions.Messaging;

namespace NexusMail.Application.Features.Automation.Commands.DisableAutomation;

public record DisableAutomationCommand(Guid RuleId, Guid WorkspaceId) : ICommand<Result>;
