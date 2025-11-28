using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;
using Afina.Modules.Users.Shared.Services;

namespace Afina.Modules.Users.Features.Login;

public sealed class LoginHandler : IRequestHandler<LoginRequest, LoginResponse>
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;
    private readonly IUserSessionsRepository _sessions;

    public LoginHandler(IUserRepository users, ITokenService tokens, IUserSessionsRepository sessions)
        => (_users, _tokens, _sessions) = (users, tokens, sessions);

    public async Task<LoginResponse> HandleAsync(LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.AuthHash))
            throw new UnauthorizedAccessException();

        var user = await _users.GetByUsernameAsync(request.Username, ct) ?? throw new UnauthorizedAccessException();
        var ok = await _users.VerifyAuthHashAsync(user, request.AuthHash, ct);
        if (!ok) throw new UnauthorizedAccessException();

        var access = _tokens.CreateAccessToken(user.Id, user.Username);
        var refresh = _tokens.CreateRefreshToken(user.Id);
        await _sessions.CreateSessionAsync(user.Id, refresh, ct);

        return new LoginResponse
        {
            Token = access,
            RefreshToken = refresh,
            User = new { id = user.Id, username = user.Username }
        };
    }
}
