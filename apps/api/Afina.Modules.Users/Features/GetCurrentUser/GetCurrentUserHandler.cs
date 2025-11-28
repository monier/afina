using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;

namespace Afina.Modules.Users.Features.GetCurrentUser;

public sealed class GetCurrentUserHandler : IRequestHandler<GetCurrentUserRequest, GetCurrentUserResponse>
{
    private readonly IUserRepository _users;
    public GetCurrentUserHandler(IUserRepository users) => _users = users;

    public async Task<GetCurrentUserResponse> HandleAsync(GetCurrentUserRequest request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct);
        if (user is null) throw new UnauthorizedAccessException();
        return new GetCurrentUserResponse { Id = user.Id, Username = user.Username };
    }
}
