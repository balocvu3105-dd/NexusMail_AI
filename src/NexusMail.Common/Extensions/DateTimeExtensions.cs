namespace NexusMail.Common.Extensions;

/// <summary>
/// DateTimeOffset and DateTime extension methods — no external dependencies.
/// </summary>
public static class DateTimeExtensions
{
    public static bool IsExpired(this DateTimeOffset expiry) => DateTimeOffset.UtcNow >= expiry;

    public static bool IsExpiredWithBuffer(this DateTimeOffset expiry, TimeSpan buffer)
        => DateTimeOffset.UtcNow >= expiry.Subtract(buffer);

    public static DateTimeOffset StartOfDay(this DateTimeOffset dt)
        => new(dt.Year, dt.Month, dt.Day, 0, 0, 0, dt.Offset);

    public static DateTimeOffset EndOfDay(this DateTimeOffset dt)
        => new(dt.Year, dt.Month, dt.Day, 23, 59, 59, 999, dt.Offset);

    public static string ToIso8601(this DateTimeOffset dt)
        => dt.ToString("yyyy-MM-ddTHH:mm:ssZ");

    public static long ToUnixMilliseconds(this DateTimeOffset dt)
        => dt.ToUnixTimeMilliseconds();

    public static bool IsBetween(this DateTimeOffset dt, DateTimeOffset start, DateTimeOffset end)
        => dt >= start && dt <= end;

    public static TimeSpan Age(this DateTimeOffset dt)
        => DateTimeOffset.UtcNow - dt;
}
