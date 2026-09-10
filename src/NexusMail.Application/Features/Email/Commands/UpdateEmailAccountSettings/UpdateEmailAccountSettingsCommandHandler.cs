using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Email.Commands.UpdateEmailAccountSettings;

public sealed class UpdateEmailAccountSettingsCommandHandler : IRequestHandler<UpdateEmailAccountSettingsCommand, Result>
{
    private readonly IEmailAccountRepository _repository;
    private readonly IWorkspaceContext _workspaceContext;

    public UpdateEmailAccountSettingsCommandHandler(IEmailAccountRepository repository, IWorkspaceContext workspaceContext)
    {
        _repository = repository;
        _workspaceContext = workspaceContext;
    }

    public async Task<Result> Handle(UpdateEmailAccountSettingsCommand request, CancellationToken cancellationToken)
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

        account.SetSyncSettings(request.SyncEnabled);
        
        await _repository.UpdateAsync(account, cancellationToken);

        return Result.Success();
    }
}
