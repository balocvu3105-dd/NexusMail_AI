using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Features.AI.Services;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.AI.Commands.ProcessEmailAI;

public sealed class ProcessEmailAICommandHandler : IRequestHandler<ProcessEmailAICommand, Result<Guid>>
{
    private readonly IAIWorkflowManager _workflowManager;

    public ProcessEmailAICommandHandler(IAIWorkflowManager workflowManager)
    {
        _workflowManager = workflowManager;
    }

    public async Task<Result<Guid>> Handle(ProcessEmailAICommand request, CancellationToken cancellationToken)
    {
        return await _workflowManager.ProcessEmailAIAsync(request.EmailId, request.WorkspaceId, cancellationToken);
    }
}
