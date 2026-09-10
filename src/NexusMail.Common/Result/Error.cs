namespace NexusMail.Common.Result;

/// <summary>
/// Represents a structured error with a code and description.
/// Immutable record — safe to share across threads.
/// </summary>
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error Unexpected = new("Error.Unexpected", "An unexpected error occurred.");
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    // Factory helpers
    public static Error NotFound(string resource, object id)
        => new($"{resource}.NotFound", $"{resource} with id '{id}' was not found.");

    public static Error Validation(string field, string message)
        => new($"Validation.{field}", message);

    public static Error Conflict(string resource, string message)
        => new($"{resource}.Conflict", message);

    public static Error Unauthorized(string message = "Unauthorized access.")
        => new("Auth.Unauthorized", message);

    public static Error Forbidden(string message = "Access is forbidden.")
        => new("Auth.Forbidden", message);

    public static Error External(string provider, string message)
        => new($"External.{provider}", message);

    public override string ToString() => $"[{Code}] {Description}";
}
