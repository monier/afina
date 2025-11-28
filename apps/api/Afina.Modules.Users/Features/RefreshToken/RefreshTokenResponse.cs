namespace Afina.Modules.Users.Features.RefreshToken;

public sealed class RefreshTokenResponse
{
    public string Token { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}
