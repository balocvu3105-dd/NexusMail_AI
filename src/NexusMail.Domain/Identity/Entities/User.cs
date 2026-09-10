using System;
using NexusMail.Shared.Domain;
using NexusMail.Domain.Identity.Events;

namespace NexusMail.Domain.Identity.Entities;

public sealed class User : AggregateRoot<Guid>
{
    private User(Guid id, string email, string firstName, string lastName, string passwordHash) : base(id)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PasswordHash = passwordHash;
        IsActive = true;
    }

    private User() { } // For EF Core

    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public static User Create(string email, string firstName, string lastName, string passwordHash)
    {
        Guard.Against.Empty(email);
        Guard.Against.Empty(firstName);
        Guard.Against.Empty(passwordHash);

        var user = new User(Guid.NewGuid(), email, firstName, lastName, passwordHash);
        
        user.AddDomainEvent(new UserRegistered { UserId = user.Id, Email = email });

        return user;
    }
}
