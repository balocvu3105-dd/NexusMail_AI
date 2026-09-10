using System;
using NexusMail.Shared.Domain;
using NexusMail.Domain.Email.Enums;
using NexusMail.Domain.Email.Events;

namespace NexusMail.Domain.Email.Entities;

public class EmailAccount : AggregateRoot
{
    public Guid WorkspaceId { get; private set; }
    public EmailProvider Provider { get; private set; }
    public string EmailAddress { get; private set; } = string.Empty;
    public string EncryptedAccessToken { get; private set; } = string.Empty;
    public string EncryptedRefreshToken { get; private set; } = string.Empty;
    public string EncryptionVersion { get; private set; } = "v1";
    public DateTime? TokenExpiresAt { get; private set; }
    public EmailAccountStatus Status { get; private set; }
    public bool SyncEnabled { get; private set; }
    public DateTime? LastSyncAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    public EmailSynchronizationState? State { get; private set; }
    
    // Concurrency Token
    public uint Version { get; private set; }

    private EmailAccount() { }

    public static EmailAccount Create(
        Guid workspaceId,
        EmailProvider provider,
        string emailAddress,
        string encryptedAccessToken,
        string encryptedRefreshToken,
        DateTime? tokenExpiresAt,
        string encryptionVersion = "v1")
    {
        var account = new EmailAccount
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Provider = provider,
            EmailAddress = emailAddress,
            EncryptedAccessToken = encryptedAccessToken,
            EncryptedRefreshToken = encryptedRefreshToken,
            TokenExpiresAt = tokenExpiresAt,
            EncryptionVersion = encryptionVersion,
            Status = EmailAccountStatus.Connected,
            SyncEnabled = true,
            CreatedAt = DateTime.UtcNow,
            State = EmailSynchronizationState.Create(Guid.Empty) // Will update ID below
        };

        account.State = EmailSynchronizationState.Create(account.Id);

        account.AddDomainEvent(new EmailAccountConnectedEvent(account.Id, workspaceId));

        return account;
    }

    public void UpdateTokens(
        string encryptedAccessToken, 
        string encryptedRefreshToken, 
        DateTime? tokenExpiresAt,
        string encryptionVersion)
    {
        EncryptedAccessToken = encryptedAccessToken;
        EncryptedRefreshToken = encryptedRefreshToken;
        TokenExpiresAt = tokenExpiresAt;
        EncryptionVersion = encryptionVersion;
        Status = EmailAccountStatus.Connected;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsError()
    {
        Status = EmailAccountStatus.Error;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disconnect()
    {
        Status = EmailAccountStatus.Disconnected;
        SyncEnabled = false;
        EncryptedAccessToken = string.Empty;
        EncryptedRefreshToken = string.Empty;
        TokenExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new EmailAccountDisconnectedEvent(Id));
    }

    public void SetSyncSettings(bool enabled)
    {
        SyncEnabled = enabled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLastSyncAt(DateTime lastSyncAt)
    {
        LastSyncAt = lastSyncAt;
    }
}
