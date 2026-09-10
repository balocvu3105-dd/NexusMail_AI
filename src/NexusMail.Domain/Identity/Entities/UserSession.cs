using System;
using NexusMail.Shared.Domain;

namespace NexusMail.Domain.Identity.Entities;

public sealed class UserSession : AggregateRoot<Guid>
{
    private UserSession(Guid id, Guid userId, string refreshTokenHash, DateTimeOffset createdAtUtc, DateTimeOffset expiresAtUtc, string? deviceName, string? ipAddress, string? userAgent) 
        : base(id)
    {
        UserId = userId;
        RefreshTokenHash = refreshTokenHash;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        DeviceName = deviceName;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    private UserSession() { } // For EF Core

    public Guid UserId { get; private set; }
    public string RefreshTokenHash { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }
    public string? DeviceName { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    public static UserSession Create(Guid userId, string refreshTokenHash, DateTimeOffset createdAtUtc, DateTimeOffset expiresAtUtc, string? deviceName = null, string? ipAddress = null, string? userAgent = null)
    {
        Guard.Against.Empty(refreshTokenHash);

        return new UserSession(Guid.NewGuid(), userId, refreshTokenHash, createdAtUtc, expiresAtUtc, deviceName, ipAddress, userAgent);
    }

    public void Revoke(DateTimeOffset revokedAtUtc)
    {
        if (RevokedAtUtc.HasValue) return;
        RevokedAtUtc = revokedAtUtc;
    }

    public bool IsActive(DateTimeOffset utcNow)
    {
        return !RevokedAtUtc.HasValue && utcNow < ExpiresAtUtc;
    }

    public void Rotate(string newRefreshTokenHash, DateTimeOffset newExpiresAtUtc)
    {
        Guard.Against.Empty(newRefreshTokenHash);
        
        RefreshTokenHash = newRefreshTokenHash;
        ExpiresAtUtc = newExpiresAtUtc;
    }
}
