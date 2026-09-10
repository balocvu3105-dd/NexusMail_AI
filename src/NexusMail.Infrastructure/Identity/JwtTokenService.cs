using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Application.Features.Identity.Results;

namespace NexusMail.Infrastructure.Identity;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public AuthenticationResult GenerateTokens(Guid userId, string email)
    {
        var accessToken = GenerateAccessToken(userId, email);
        var refreshToken = GenerateRefreshToken();
        
        var expiresAtUtc = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        return new AuthenticationResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string GenerateAccessToken(Guid userId, string email)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
