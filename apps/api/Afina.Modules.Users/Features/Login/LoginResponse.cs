namespace Afina.Modules.Users.Features.Login;

public sealed class LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public Guid UserId { get; init; }
}
