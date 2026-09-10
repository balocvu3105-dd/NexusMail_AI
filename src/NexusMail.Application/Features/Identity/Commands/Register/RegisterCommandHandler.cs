using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Domain.Identity.Entities;
using NexusMail.Domain.Identity.Repositories;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Identity.Commands.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            return Result.Failure<Guid>(new Error("Identity.DuplicateEmail", "The email address is already in use."));
        }

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = User.Create(
            request.Email,
            request.FirstName,
            request.LastName,
            passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);

        return Result.Success(user.Id);
    }
}
