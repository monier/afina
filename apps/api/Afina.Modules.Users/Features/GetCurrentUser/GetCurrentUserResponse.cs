using System;

namespace Afina.Modules.Users.Features.GetCurrentUser;

public sealed class GetCurrentUserResponse
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
}
