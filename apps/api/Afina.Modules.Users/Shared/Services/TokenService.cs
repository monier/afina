using System;
using System.Security.Cryptography;

namespace Afina.Modules.Users.Shared.Services;

public sealed class TokenService : ITokenService
{
    public string CreateAccessToken(Guid userId, string username)
    {
        // TODO: replace with JWT implementation
        return $"access-{userId}-{username}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
    }

    public string CreateRefreshToken(Guid userId)
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }
}
