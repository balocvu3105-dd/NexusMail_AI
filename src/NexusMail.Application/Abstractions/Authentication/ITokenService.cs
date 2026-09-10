using System;
using NexusMail.Application.Features.Identity.Results;

namespace NexusMail.Application.Abstractions.Authentication;

public interface ITokenService
{
    AuthenticationResult GenerateTokens(Guid userId, string email);
    string GenerateRefreshToken();
}
