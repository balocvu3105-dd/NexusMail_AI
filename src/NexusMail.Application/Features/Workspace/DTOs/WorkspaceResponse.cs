using System;

namespace NexusMail.Application.Features.Workspace.DTOs;

public record WorkspaceResponse(
    Guid Id,
    string Name,
    string Plan,
    Guid OwnerId
);
