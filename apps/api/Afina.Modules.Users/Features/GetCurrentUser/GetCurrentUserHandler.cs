using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Core.Api;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;
using Microsoft.Extensions.Logging;

namespace Afina.Modules.Users.Features.GetCurrentUser;

public sealed class GetCurrentUserHandler : IRequestHandler<GetCurrentUserRequest, GetCurrentUserResponse>
{
    private readonly IUserRepository _users;
    private readonly ILogger<GetCurrentUserHandler> _logger;

    public GetCurrentUserHandler(IUserRepository users, ILogger<GetCurrentUserHandler> logger)
    {
        _users = users;
        _logger = logger;
    }

    public async Task<GetCurrentUserResponse> HandleAsync(GetCurrentUserRequest request, CancellationToken ct)
    {
        _logger.LogDebug("Fetching current user for UserId: {UserId}", request.UserId);

        var user = await _users.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found", request.UserId);
            throw new ApiException(ErrorCodes.USER_DELETED, "User account no longer exists.");
        }

        return new GetCurrentUserResponse { Id = user.Id, Username = user.Username };
    }
}
