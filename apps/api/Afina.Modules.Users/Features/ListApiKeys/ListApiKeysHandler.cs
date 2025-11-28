using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;

namespace Afina.Modules.Users.Features.ListApiKeys;

public sealed class ListApiKeysHandler : IRequestHandler<ListApiKeysRequest, ListApiKeysResponse>
{
    private readonly IApiKeyRepository _repo;
    public ListApiKeysHandler(IApiKeyRepository repo) => _repo = repo;

    public async Task<ListApiKeysResponse> HandleAsync(ListApiKeysRequest request, CancellationToken ct)
    {
        var keys = await _repo.GetUserApiKeysAsync(request.UserId, ct);
        return new ListApiKeysResponse { Keys = keys.Select(k => new { id = k.Id, name = k.Name, prefix = k.KeyPrefix }).Cast<object>().ToList() };
    }
}
