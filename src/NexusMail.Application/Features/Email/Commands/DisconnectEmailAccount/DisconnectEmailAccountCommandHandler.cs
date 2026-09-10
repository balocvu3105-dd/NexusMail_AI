using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Email.Commands.DisconnectEmailAccount;

public sealed class DisconnectEmailAccountCommandHandler : IRequestHandler<DisconnectEmailAccountCommand, Result>
{
    private readonly IEmailAccountRepository _repository;
    private readonly IWorkspaceContext _workspaceContext;

    public DisconnectEmailAccountCommandHandler(IEmailAccountRepository repository, IWorkspaceContext workspaceContext)
    {
        _repository = repository;
        _workspaceContext = workspaceContext;
    }

    public async Task<Result> Handle(DisconnectEmailAccountCommand request, CancellationToken cancellationToken)
    {
        if (!_workspaceContext.WorkspaceId.HasValue)
        {
            return Result.Failure(new Error("Unauthorized", "Workspace context not found."));
        }

        var account = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (account == null || account.WorkspaceId != _workspaceContext.WorkspaceId.Value)
        {
            return Result.Failure(new Error("NotFound", "Email account not found."));
        }

        account.Disconnect();
        
        await _repository.UpdateAsync(account, cancellationToken);
        // Or if the design is hard delete, call _repository.DeleteAsync(account, cancellationToken);
        // Disconnect seems like soft delete / status change based on Domain Entity changes

        return Result.Success();
    }
}
