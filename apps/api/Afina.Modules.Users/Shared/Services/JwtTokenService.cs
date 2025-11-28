using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Afina.Modules.Users.Shared.Services;

public sealed class JwtTokenService : ITokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SymmetricSecurityKey _key;

    public JwtTokenService(IConfiguration config)
    {
        _issuer = config["Jwt:Issuer"] ?? "Afina";
        _audience = config["Jwt:Audience"] ?? "AfinaClients";
        var signing = config["Jwt:SigningKey"] ?? "local-dev-signing-key-change-me";
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signing));
    }

    public string CreateAccessToken(Guid userId, string username)
    {
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username)
        };
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateRefreshToken(Guid userId)
    {
        // For now, reuse random approach; storage handled elsewhere
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }
}
