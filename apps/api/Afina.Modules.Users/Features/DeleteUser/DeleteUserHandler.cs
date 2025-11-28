using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;

namespace Afina.Modules.Users.Features.DeleteUser;

public sealed class DeleteUserHandler : IRequestHandler<DeleteUserRequest, EmptyResponse>
{
    private readonly IUserRepository _users;
    private readonly IUserSessionsRepository _sessions;
    public DeleteUserHandler(IUserRepository users, IUserSessionsRepository sessions)
        => (_users, _sessions) = (users, sessions);

    public async Task<EmptyResponse> HandleAsync(DeleteUserRequest request, CancellationToken ct)
    {
        await _sessions.RevokeAllUserSessionsAsync(request.UserId, ct);
        var user = await _users.GetByIdAsync(request.UserId, ct);
        if (user is not null)
        {
            await _users.DeleteUserAsync(user, ct);
        }
        return EmptyResponse.Value;
    }
}
