namespace NexusMail.Domain.Email.Enums;

public enum EmailSyncStage
{
    Pending = 0,
    RefreshingToken = 1,
    FetchingProfile = 2,
    FetchingLabels = 3,
    FetchingMessages = 4,
    Mapping = 5,
    Persisting = 6,
    Completed = 7,
    Failed = 8
}
