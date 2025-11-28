namespace Afina.Modules.Users.Features.Register;

public sealed class RegisterResponse
{
    public string Token { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public object User { get; init; } = default!;
}
