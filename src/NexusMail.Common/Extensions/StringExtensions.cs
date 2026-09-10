namespace NexusMail.Common.Extensions;

/// <summary>
/// String extension methods — no external dependencies.
/// </summary>
public static class StringExtensions
{
    public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);

    public static string Truncate(this string value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;
        return string.Concat(value.AsSpan(0, maxLength - suffix.Length), suffix);
    }

    public static string ToSnakeCase(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        var result = System.Text.RegularExpressions.Regex.Replace(
            value, @"(?<!^)([A-Z])", "_$1");
        return result.ToLowerInvariant();
    }

    public static string MaskEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return email;
        var atIndex = email.IndexOf('@');
        if (atIndex <= 1) return email;
        return string.Concat(email.AsSpan(0, 2), new string('*', atIndex - 2), email.AsSpan(atIndex));
    }

    public static string? NullIfWhiteSpace(this string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value;
}
