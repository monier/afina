using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;
using Afina.Modules.Users.Shared.Services;
using Microsoft.Extensions.Logging;

namespace Afina.Modules.Users.Features.Login;

public sealed class LoginHandler : IRequestHandler<LoginRequest, LoginResponse>
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;
    private readonly IUserSessionsRepository _sessions;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUserRepository users,
        ITokenService tokens,
        IUserSessionsRepository sessions,
        ILogger<LoginHandler> logger)
    {
        _users = users;
        _tokens = tokens;
        _sessions = sessions;
        _logger = logger;
    }

    public async Task<LoginResponse> HandleAsync(LoginRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Login attempt for username: {Username}", request.Username);

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.AuthHash))
        {
            _logger.LogWarning("Login failed: Invalid username or password hash");
            throw new UnauthorizedAccessException();
        }

        var user = await _users.GetByUsernameAsync(request.Username, ct);
        if (user == null)
        {
            _logger.LogWarning("Login failed: User {Username} not found", request.Username);
            throw new UnauthorizedAccessException();
        }

        var ok = await _users.VerifyAuthHashAsync(user, request.AuthHash, ct);
        if (!ok)
        {
            _logger.LogWarning("Login failed: Invalid password for user {Username}", request.Username);
            throw new UnauthorizedAccessException();
        }

        var access = _tokens.CreateAccessToken(user.Id, user.Username);
        var refresh = _tokens.CreateRefreshToken(user.Id);
        await _sessions.CreateSessionAsync(user.Id, refresh, ct);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return new LoginResponse
        {
            Token = access,
            RefreshToken = refresh,
            User = new { id = user.Id, username = user.Username }
        };
    }
}
