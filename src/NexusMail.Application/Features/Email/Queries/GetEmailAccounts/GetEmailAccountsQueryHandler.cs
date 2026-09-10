using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Application.Features.Email.DTOs;
using NexusMail.Shared.Common;
using NexusMail.Shared.Pagination;

namespace NexusMail.Application.Features.Email.Queries.GetEmailAccounts;

public sealed class GetEmailAccountsQueryHandler : IRequestHandler<GetEmailAccountsQuery, Result<PagedResult<EmailAccountDto>>>
{
    private readonly IEmailAccountRepository _repository;
    private readonly IWorkspaceContext _workspaceContext;

    public GetEmailAccountsQueryHandler(IEmailAccountRepository repository, IWorkspaceContext workspaceContext)
    {
        _repository = repository;
        _workspaceContext = workspaceContext;
    }

    public async Task<Result<PagedResult<EmailAccountDto>>> Handle(GetEmailAccountsQuery request, CancellationToken cancellationToken)
    {
        if (!_workspaceContext.WorkspaceId.HasValue)
        {
            return Result.Failure<PagedResult<EmailAccountDto>>(new Error("Unauthorized", "Workspace context not found."));
        }

        var (accounts, totalCount) = await _repository.GetPagedByWorkspaceIdAsync(
            _workspaceContext.WorkspaceId.Value, 
            request.PageRequest.Page, 
            request.PageRequest.PageSize, 
            cancellationToken);

        var dtos = accounts.Select(a => new EmailAccountDto(
            a.Id,
            a.Provider,
            a.EmailAddress,
            a.Status,
            a.SyncEnabled,
            a.LastSyncAt,
            a.CreatedAt
        )).ToList();

        var pagedResult = new PagedResult<EmailAccountDto>(
            dtos, 
            totalCount, 
            request.PageRequest.Page, 
            request.PageRequest.PageSize);

        return Result.Success(pagedResult);
    }
}
