using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Domain.Email.Entities;
using NexusMail.Domain.Email.Events;
using NexusMail.Domain.Email.Exceptions;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Email.Commands.ConnectEmailAccount;

public sealed class ConnectEmailAccountCommandHandler : IRequestHandler<ConnectEmailAccountCommand, Result<Guid>>
{
    private readonly IEnumerable<IOAuthProvider> _oauthProviders;
    private readonly ITokenEncryptionService _encryptionService;
    private readonly IEmailAccountRepository _repository;
    private readonly IWorkspaceContext _workspaceContext;
    private readonly IPublisher _publisher;

    public ConnectEmailAccountCommandHandler(
        IEnumerable<IOAuthProvider> oauthProviders,
        ITokenEncryptionService encryptionService,
        IEmailAccountRepository repository,
        IWorkspaceContext workspaceContext,
        IPublisher publisher)
    {
        _oauthProviders = oauthProviders;
        _encryptionService = encryptionService;
        _repository = repository;
        _workspaceContext = workspaceContext;
        _publisher = publisher;
    }

    public async Task<Result<Guid>> Handle(ConnectEmailAccountCommand request, CancellationToken cancellationToken)
    {
        if (!_workspaceContext.WorkspaceId.HasValue)
        {
            return Result.Failure<Guid>(new Error("Unauthorized", "Workspace context not found."));
        }

        var provider = _oauthProviders.FirstOrDefault(p => p.Provider == request.Provider);
        if (provider == null)
        {
            return Result.Failure<Guid>(new Error("ProviderNotSupported", $"The provider {request.Provider} is not supported."));
        }

        var exchangeResult = await provider.ExchangeCodeAsync(request.AuthCode, request.RedirectUri, cancellationToken);
        if (exchangeResult.IsFailure)
        {
            return Result.Failure<Guid>(exchangeResult.Error);
        }

        var tokens = exchangeResult.Value;

        var encryptedAccess = await _encryptionService.EncryptAsync(tokens.AccessToken, cancellationToken);
        var encryptedRefresh = await _encryptionService.EncryptAsync(tokens.RefreshToken, cancellationToken);

        var existingAccountExists = await _repository.ExistsAsync(_workspaceContext.WorkspaceId.Value, tokens.EmailAddress, cancellationToken);
        if (existingAccountExists)
        {
            return Result.Failure<Guid>(new Error("AccountExists", "This email account is already connected to the workspace."));
        }

        var emailAccount = EmailAccount.Create(
            _workspaceContext.WorkspaceId.Value,
            request.Provider,
            tokens.EmailAddress,
            encryptedAccess,
            encryptedRefresh,
            tokens.ExpiresAt,
            _encryptionService.CurrentVersion
        );

        await _repository.AddAsync(emailAccount, cancellationToken);

        // Directly publish domain event to trigger email sync (MVP: bypass outbox for immediate processing)
        await _publisher.Publish(new EmailAccountConnectedEvent(emailAccount.Id, emailAccount.WorkspaceId), cancellationToken);

        return Result.Success(emailAccount.Id);
    }
}
