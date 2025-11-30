using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;
using Afina.Modules.Users.Shared.Services;
using Microsoft.Extensions.Logging;

namespace Afina.Modules.Users.Features.RefreshToken;

public sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenRequest, RefreshTokenResponse>
{
    private readonly IUserSessionsRepository _sessions;
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        IUserSessionsRepository sessions,
        IUserRepository users,
        ITokenService tokens,
        ILogger<RefreshTokenHandler> logger)
    {
        _sessions = sessions;
        _users = users;
        _tokens = tokens;
        _logger = logger;
    }

    public async Task<RefreshTokenResponse> HandleAsync(RefreshTokenRequest request, CancellationToken ct)
    {
        _logger.LogDebug("Attempting to refresh token");

        var userId = await _sessions.ValidateRefreshTokenAsync(request.RefreshToken, ct);
        if (userId is null)
        {
            _logger.LogWarning("Invalid refresh token provided");
            throw new UnauthorizedAccessException();
        }

        var user = await _users.GetByIdAsync(userId.Value, ct);
        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found during token refresh", userId.Value);
            throw new UnauthorizedAccessException();
        }

        await _sessions.RevokeSessionAsync(request.RefreshToken, ct);
        var newRefresh = _tokens.CreateRefreshToken(userId.Value);
        await _sessions.CreateSessionAsync(userId.Value, newRefresh, ct);

        var access = _tokens.CreateAccessToken(userId.Value, user.Username);

        _logger.LogInformation("Token refreshed successfully for user {UserId}", userId.Value);

        return new RefreshTokenResponse
        {
            Token = access,
            RefreshToken = newRefresh
        };
    }
}
