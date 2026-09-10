namespace NexusMail.API.Configuration;

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimiting";

    public int LoginLimit { get; set; } = 5;
    public int LoginWindowSeconds { get; set; } = 60;

    public int RefreshLimit { get; set; } = 10;
    public int RefreshWindowSeconds { get; set; } = 60;
}
