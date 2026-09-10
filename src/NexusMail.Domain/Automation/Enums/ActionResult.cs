namespace NexusMail.Domain.Automation.Enums;

public enum ActionResult
{
    Success,
    TransientFailure,
    PermanentFailure,
    Unknown,
    Skipped
}
