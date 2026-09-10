using System;
using NexusMail.Shared.Common;
using NexusMail.Application.Abstractions.Messaging;

namespace NexusMail.Application.Features.Automation.Commands.EnableAutomation;

public record EnableAutomationCommand(Guid RuleId, Guid WorkspaceId) : ICommand<Result>;
