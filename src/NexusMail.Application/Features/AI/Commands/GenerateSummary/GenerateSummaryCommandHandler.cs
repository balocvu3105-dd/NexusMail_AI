using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Abstractions.AI;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Domain.AI.Entities;
using NexusMail.Shared.Domain;


namespace NexusMail.Application.Features.AI.Commands.GenerateSummary;

public sealed class GenerateSummaryCommandHandler : IRequestHandler<GenerateSummaryCommand, Result<Guid>>
{
    private readonly NexusMail.Application.Features.AI.Services.IAIWorkflowManager _aiWorkflowManager;

    public GenerateSummaryCommandHandler(NexusMail.Application.Features.AI.Services.IAIWorkflowManager aiWorkflowManager)
    {
        _aiWorkflowManager = aiWorkflowManager;
    }

    public async Task<Result<Guid>> Handle(GenerateSummaryCommand request, CancellationToken cancellationToken)
    {
        return await _aiWorkflowManager.GenerateSummaryAsync(request.EmailId, request.WorkspaceId, cancellationToken);
    }
}
