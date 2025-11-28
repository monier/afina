using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;
using Afina.Modules.Users.Shared.Services;

namespace Afina.Modules.Users.Features.Register;

public sealed class RegisterHandler : IRequestHandler<RegisterRequest, RegisterResponse>
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;
    private readonly IUserSessionsRepository _sessions;

    public RegisterHandler(IUserRepository users, ITokenService tokens, IUserSessionsRepository sessions)
        => (_users, _tokens, _sessions) = (users, tokens, sessions);

    public async Task<RegisterResponse> HandleAsync(RegisterRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.PasswordHash))
            throw new ArgumentException("Username and password hash are required.");

        var existing = await _users.GetByUsernameAsync(request.Username, ct);
        if (existing is not null)
            throw new InvalidOperationException("Username already exists.");

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.PasswordHash);
        var user = await _users.CreateUserAsync(request.Username, hashedPassword, request.PasswordHint, ct);

        var access = _tokens.CreateAccessToken(user.Id, user.Username);
        var refresh = _tokens.CreateRefreshToken(user.Id);
        await _sessions.CreateSessionAsync(user.Id, refresh, ct);

        return new RegisterResponse
        {
            Token = access,
            RefreshToken = refresh,
            User = new { id = user.Id, username = user.Username }
        };
    }
}
