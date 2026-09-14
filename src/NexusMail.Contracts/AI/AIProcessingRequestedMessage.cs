using System;

namespace NexusMail.Contracts.AI;

public sealed record AIProcessingRequestedMessage
{
    public Guid EmailId { get; init; }
    public Guid WorkspaceId { get; init; }
}
