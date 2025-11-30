using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;
using Afina.Modules.Users.Shared.Services;
using Microsoft.Extensions.Logging;

namespace Afina.Modules.Users.Features.Register;

public sealed class RegisterHandler : IRequestHandler<RegisterRequest, RegisterResponse>
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;
    private readonly IUserSessionsRepository _sessions;
    private readonly ILogger<RegisterHandler> _logger;

    public RegisterHandler(
        IUserRepository users,
        ITokenService tokens,
        IUserSessionsRepository sessions,
        ILogger<RegisterHandler> logger)
    {
        _users = users;
        _tokens = tokens;
        _sessions = sessions;
        _logger = logger;
    }

    public async Task<RegisterResponse> HandleAsync(RegisterRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Registration attempt for username: {Username}", request.Username);

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.PasswordHash))
        {
            _logger.LogWarning("Registration failed: Username and password are required");
            throw new ArgumentException("Username and password hash are required.");
        }

        var existing = await _users.GetByUsernameAsync(request.Username, ct);
        if (existing is not null)
        {
            _logger.LogWarning("Registration failed: Username {Username} already exists", request.Username);
            throw new InvalidOperationException("Username already exists.");
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.PasswordHash);
        var user = await _users.CreateUserAsync(request.Username, hashedPassword, request.PasswordHint, ct);

        _logger.LogInformation("User {UserId} registered successfully with username {Username}", user.Id, user.Username);

        var access = _tokens.CreateAccessToken(user.Id, user.Username);
        var refresh = _tokens.CreateRefreshToken(user.Id);
        await _sessions.CreateSessionAsync(user.Id, refresh, ct);

        _logger.LogDebug("Session created for user {UserId}", user.Id);

        return new RegisterResponse
        {
            Token = access,
            RefreshToken = refresh,
            User = new { id = user.Id, username = user.Username }
        };
    }
}
