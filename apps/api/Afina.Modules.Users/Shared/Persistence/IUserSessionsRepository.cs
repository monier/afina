using System;
using System.Threading;
using System.Threading.Tasks;

namespace Afina.Modules.Users.Shared.Persistence;

public interface IUserSessionsRepository
{
    Task<string> CreateSessionAsync(Guid userId, string refreshToken, CancellationToken ct);
    Task<Guid?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken ct);
    Task RevokeSessionAsync(string refreshToken, CancellationToken ct);
    Task RevokeAllUserSessionsAsync(Guid userId, CancellationToken ct);
}
