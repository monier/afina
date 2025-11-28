using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;

namespace Afina.Modules.Users.Features.DeleteApiKey;

public sealed class DeleteApiKeyHandler : IRequestHandler<DeleteApiKeyRequest, EmptyResponse>
{
    private readonly IApiKeyRepository _repo;
    public DeleteApiKeyHandler(IApiKeyRepository repo) => _repo = repo;

    public async Task<EmptyResponse> HandleAsync(DeleteApiKeyRequest request, CancellationToken ct)
    {
        var key = await _repo.GetApiKeyByIdAsync(request.KeyId, ct);
        if (key is null) return EmptyResponse.Value;
        await _repo.DeleteApiKeyAsync(request.KeyId, ct);
        return EmptyResponse.Value;
    }
}
