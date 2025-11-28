using Afina.Infrastructure.Mediator;

namespace Afina.Modules.Users.Features.Register;

public sealed class RegisterRequest : IRequest<RegisterResponse>
{
    public string Username { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public string? PasswordHint { get; init; }
}
