namespace NexusMail.Domain.Email.Enums;

public enum EmailSyncStatus
{
    Pending = 0,
    Idle = 1,
    Syncing = 2,
    Retrying = 3,
    Failed = 4
}
