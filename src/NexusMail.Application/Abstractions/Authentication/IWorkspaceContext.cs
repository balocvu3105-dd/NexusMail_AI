using System;

namespace NexusMail.Application.Abstractions.Authentication;

public interface IWorkspaceContext
{
    Guid? WorkspaceId { get; }
}
