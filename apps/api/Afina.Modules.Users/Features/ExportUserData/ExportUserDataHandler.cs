using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Afina.Core.Api;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;
using Microsoft.Extensions.Logging;

namespace Afina.Modules.Users.Features.ExportUserData;

public sealed class ExportUserDataHandler : IRequestHandler<ExportUserDataRequest, ExportUserDataResponse>
{
    private readonly IUserRepository _users;
    private readonly ILogger<ExportUserDataHandler> _logger;

    public ExportUserDataHandler(IUserRepository users, ILogger<ExportUserDataHandler> logger)
    {
        _users = users;
        _logger = logger;
    }

    public async Task<ExportUserDataResponse> HandleAsync(ExportUserDataRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Exporting data for user {UserId}", request.UserId);

        var user = await _users.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found for data export", request.UserId);
            throw new ApiException(ErrorCodes.UNAUTHORIZED, "Unauthorized access.");
        }

        var payload = new { id = user.Id, username = user.Username };
        var data = JsonSerializer.Serialize(payload);

        _logger.LogInformation("User data exported successfully for user {UserId}", request.UserId);

        return new ExportUserDataResponse { Data = data };
    }
}
