namespace NexusMail.Common.Guards;

/// <summary>
/// Lightweight guard clauses — throws on invalid input.
/// No external dependencies.
/// </summary>
public static class Guard
{
    public static T NotNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(paramName, $"'{paramName}' must not be null.");
        return value;
    }

    public static string NotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"'{paramName}' must not be null or whitespace.", paramName);
        return value;
    }

    public static Guid NotEmpty(Guid value, string paramName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"'{paramName}' must not be an empty Guid.", paramName);
        return value;
    }

    public static int Positive(int value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName, $"'{paramName}' must be positive.");
        return value;
    }

    public static int NotNegative(int value, string paramName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, $"'{paramName}' must not be negative.");
        return value;
    }

    public static string MaxLength(string value, int maxLength, string paramName)
    {
        if (value.Length > maxLength)
            throw new ArgumentException($"'{paramName}' must not exceed {maxLength} characters.", paramName);
        return value;
    }

    public static T IsTrue<T>(T value, Func<T, bool> predicate, string paramName, string message)
    {
        if (!predicate(value))
            throw new ArgumentException(message, paramName);
        return value;
    }

    /// <summary>
    /// Returns a Result.Failure instead of throwing — useful in domain methods.
    /// </summary>
    public static NexusMail.Common.Result.Result AgainstNullOrEmpty(string? value, string code, string description)
        => string.IsNullOrWhiteSpace(value)
            ? NexusMail.Common.Result.Result.Failure(new NexusMail.Common.Result.Error(code, description))
            : NexusMail.Common.Result.Result.Success();
}
