using Microsoft.AspNetCore.Identity;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Domain.Identity.Entities;

namespace NexusMail.Infrastructure.Identity;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _passwordHasher;

    public PasswordHasher()
    {
        _passwordHasher = new PasswordHasher<User>();
    }

    public string Hash(string password)
    {
        return _passwordHasher.HashPassword(null!, password);
    }

    public bool Verify(string password, string passwordHash)
    {
        var result = _passwordHasher.VerifyHashedPassword(null!, passwordHash, password);
        return result != PasswordVerificationResult.Failed;
    }
}
