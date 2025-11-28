using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;
using Afina.Modules.Users.Shared.Services;

namespace Afina.Modules.Users.Features.RefreshToken;

public sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenRequest, RefreshTokenResponse>
{
    private readonly IUserSessionsRepository _sessions;
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;

    public RefreshTokenHandler(IUserSessionsRepository sessions, IUserRepository users, ITokenService tokens)
        => (_sessions, _users, _tokens) = (sessions, users, tokens);

    public async Task<RefreshTokenResponse> HandleAsync(RefreshTokenRequest request, CancellationToken ct)
    {
        var userId = await _sessions.ValidateRefreshTokenAsync(request.RefreshToken, ct);
        if (userId is null) throw new UnauthorizedAccessException();

        var user = await _users.GetByIdAsync(userId.Value, ct);
        if (user is null) throw new UnauthorizedAccessException();

        await _sessions.RevokeSessionAsync(request.RefreshToken, ct);
        var newRefresh = _tokens.CreateRefreshToken(userId.Value);
        await _sessions.CreateSessionAsync(userId.Value, newRefresh, ct);

        var access = _tokens.CreateAccessToken(userId.Value, user.Username);

        return new RefreshTokenResponse
        {
            Token = access,
            RefreshToken = newRefresh
        };
    }
}
