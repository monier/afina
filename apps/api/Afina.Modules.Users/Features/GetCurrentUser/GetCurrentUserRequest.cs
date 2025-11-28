using System;
using Afina.Infrastructure.Mediator;

namespace Afina.Modules.Users.Features.GetCurrentUser;

public sealed class GetCurrentUserRequest : IRequest<GetCurrentUserResponse>
{
    public Guid UserId { get; init; }
}
