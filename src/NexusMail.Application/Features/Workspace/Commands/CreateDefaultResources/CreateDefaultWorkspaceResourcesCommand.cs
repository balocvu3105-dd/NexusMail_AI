using System;
using NexusMail.Application.Abstractions.Messaging;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Workspace.Commands.CreateDefaultResources;

public record CreateDefaultWorkspaceResourcesCommand(
    Guid WorkspaceId,
    Guid OwnerId
) : ICommand<Result>;
