using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Application.Abstractions.Copilot;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Copilot;

public sealed record ActionExecutionRequest
{
    public Guid WorkspaceId { get; init; }
    public string IdempotencyKey { get; init; } = string.Empty;
    public ActionProposal Proposal { get; init; } = default!;
}

public interface ICopilotActionService
{
    Task<Result> ExecuteActionAsync(ActionExecutionRequest request, CancellationToken cancellationToken = default);
}
