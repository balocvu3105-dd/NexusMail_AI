using System;

namespace NexusMail.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid? UserId { get; }
    Guid? WorkspaceId { get; }
    bool IsAuthenticated { get; }
}
