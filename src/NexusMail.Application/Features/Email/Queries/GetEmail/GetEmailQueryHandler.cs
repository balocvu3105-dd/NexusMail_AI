using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.Email.Queries.GetEmail;

public sealed class GetEmailQueryHandler : IRequestHandler<GetEmailQuery, Result<EmailDetailsDto>>
{
    private readonly IEmailRepository _emailRepository;
    private readonly IWorkspaceContext _workspaceContext;

    public GetEmailQueryHandler(IEmailRepository emailRepository, IWorkspaceContext workspaceContext)
    {
        _emailRepository = emailRepository;
        _workspaceContext = workspaceContext;
    }

    public async Task<Result<EmailDetailsDto>> Handle(GetEmailQuery request, CancellationToken cancellationToken)
    {
        var workspaceId = _workspaceContext.WorkspaceId;
        if (!workspaceId.HasValue)
        {
            return Result.Failure<EmailDetailsDto>(new Error("Workspace.Unauthorized", "No active workspace context."));
        }

        var result = await _emailRepository.GetEmailWithAnalysisAsync(request.Id, workspaceId.Value, cancellationToken);
        
        if (result == null)
        {
            return Result.Failure<EmailDetailsDto>(new Error("Email.NotFound", $"Email with ID {request.Id} not found in this workspace."));
        }

        var email = result.Value.Email;
        var analysis = result.Value.Analysis;

        var dto = new EmailDetailsDto(
            email.Id,
            email.MessageId,
            email.Sender,
            email.Subject,
            email.Content,
            email.ReceivedAt,
            analysis?.Category,
            analysis?.Language,
            analysis?.Confidence,
            analysis?.Summary,
            analysis?.PriorityScore,
            analysis?.Priority,
            analysis?.Tags
        );

        return Result.Success(dto);
    }
}
