using Afina.Infrastructure.Mediator;

namespace Afina.Modules.Users.Features.Login;

public sealed class LoginRequest : IRequest<LoginResponse>
{
    public string Username { get; init; } = string.Empty;
    public string AuthHash { get; init; } = string.Empty;
}
