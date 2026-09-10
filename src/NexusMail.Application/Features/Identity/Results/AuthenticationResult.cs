using System;

namespace NexusMail.Application.Features.Identity.Results;

public sealed record AuthenticationResult
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; init; }
}
