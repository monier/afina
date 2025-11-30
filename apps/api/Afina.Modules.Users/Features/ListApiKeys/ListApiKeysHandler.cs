using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;
using Microsoft.Extensions.Logging;

namespace Afina.Modules.Users.Features.ListApiKeys;

public sealed class ListApiKeysHandler : IRequestHandler<ListApiKeysRequest, ListApiKeysResponse>
{
    private readonly IApiKeyRepository _repo;
    private readonly ILogger<ListApiKeysHandler> _logger;

    public ListApiKeysHandler(IApiKeyRepository repo, ILogger<ListApiKeysHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<ListApiKeysResponse> HandleAsync(ListApiKeysRequest request, CancellationToken ct)
    {
        _logger.LogDebug("Listing API keys for user {UserId}", request.UserId);

        var keys = await _repo.GetUserApiKeysAsync(request.UserId, ct);

        _logger.LogDebug("Found {KeyCount} API keys for user {UserId}", keys.Count, request.UserId);

        return new ListApiKeysResponse { Keys = keys.Select(k => new { id = k.Id, name = k.Name, prefix = k.KeyPrefix }).Cast<object>().ToList() };
    }
}
