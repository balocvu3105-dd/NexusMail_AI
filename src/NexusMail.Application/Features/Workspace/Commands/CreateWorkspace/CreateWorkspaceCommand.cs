using System;
using NexusMail.Application.Abstractions.Messaging;
using NexusMail.Shared.Common;
using NexusMail.Application.Features.Workspace.DTOs;

namespace NexusMail.Application.Features.Workspace.Commands.CreateWorkspace;

public record CreateWorkspaceCommand(
    string Name,
    string Plan
) : ICommand<Result<WorkspaceResponse>>;
