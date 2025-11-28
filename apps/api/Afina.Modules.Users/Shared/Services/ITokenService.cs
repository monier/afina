using System;

namespace Afina.Modules.Users.Shared.Services;

public interface ITokenService
{
    string CreateAccessToken(Guid userId, string username);
    string CreateRefreshToken(Guid userId);
}
