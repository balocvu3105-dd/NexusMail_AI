namespace NexusMail.Application.Abstractions.Outbox;

public enum FailureReason
{
    Unknown = 0,
    OAuthExpired = 1,
    Google429 = 2,
    JsonSerialization = 3,
    HandlerException = 4
}
