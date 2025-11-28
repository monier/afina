using Afina.Infrastructure.Mediator;

namespace Afina.Modules.Users.Features.RefreshToken;

public sealed class RefreshTokenRequest : IRequest<RefreshTokenResponse>
{
    public string RefreshToken { get; init; } = string.Empty;
}
