using System.ComponentModel.DataAnnotations;

namespace NexusMail.Infrastructure.Identity;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    [Required]
    public string Secret { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ExpirationMinutes { get; set; }

    [Range(1, int.MaxValue)]
    public int RefreshTokenExpirationDays { get; set; }
}
