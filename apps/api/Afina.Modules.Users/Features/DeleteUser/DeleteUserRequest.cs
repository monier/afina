using System;
using Afina.Infrastructure.Mediator;

namespace Afina.Modules.Users.Features.DeleteUser;

public sealed class DeleteUserRequest : IRequest<EmptyResponse>
{
    public Guid UserId { get; init; }
}
