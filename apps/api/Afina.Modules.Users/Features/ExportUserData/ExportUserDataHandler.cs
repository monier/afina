using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;

namespace Afina.Modules.Users.Features.ExportUserData;

public sealed class ExportUserDataHandler : IRequestHandler<ExportUserDataRequest, ExportUserDataResponse>
{
    private readonly IUserRepository _users;
    public ExportUserDataHandler(IUserRepository users) => _users = users;

    public async Task<ExportUserDataResponse> HandleAsync(ExportUserDataRequest request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct) ?? throw new UnauthorizedAccessException();
        var payload = new { id = user.Id, username = user.Username };
        return new ExportUserDataResponse { Data = JsonSerializer.Serialize(payload) };
    }
}
