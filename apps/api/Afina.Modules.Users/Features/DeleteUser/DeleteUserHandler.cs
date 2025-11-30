using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;
using Microsoft.Extensions.Logging;

namespace Afina.Modules.Users.Features.DeleteUser;

public sealed class DeleteUserHandler : IRequestHandler<DeleteUserRequest, EmptyResponse>
{
    private readonly IUserRepository _users;
    private readonly IUserSessionsRepository _sessions;
    private readonly ILogger<DeleteUserHandler> _logger;

    public DeleteUserHandler(
        IUserRepository users,
        IUserSessionsRepository sessions,
        ILogger<DeleteUserHandler> logger)
    {
        _users = users;
        _sessions = sessions;
        _logger = logger;
    }

    public async Task<EmptyResponse> HandleAsync(DeleteUserRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Deleting user {UserId}", request.UserId);

        await _sessions.RevokeAllUserSessionsAsync(request.UserId, ct);
        _logger.LogDebug("Revoked all sessions for user {UserId}", request.UserId);

        var user = await _users.GetByIdAsync(request.UserId, ct);
        if (user is not null)
        {
            await _users.DeleteUserAsync(user, ct);
            _logger.LogInformation("User {UserId} deleted successfully", request.UserId);
        }
        else
        {
            _logger.LogWarning("User {UserId} not found for deletion", request.UserId);
        }

        return EmptyResponse.Value;
    }
}
