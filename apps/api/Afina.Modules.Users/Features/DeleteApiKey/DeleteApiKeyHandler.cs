using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;
using Microsoft.Extensions.Logging;

namespace Afina.Modules.Users.Features.DeleteApiKey;

public sealed class DeleteApiKeyHandler : IRequestHandler<DeleteApiKeyRequest, EmptyResponse>
{
    private readonly IApiKeyRepository _repo;
    private readonly ILogger<DeleteApiKeyHandler> _logger;

    public DeleteApiKeyHandler(IApiKeyRepository repo, ILogger<DeleteApiKeyHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<EmptyResponse> HandleAsync(DeleteApiKeyRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Deleting API key {KeyId}", request.KeyId);

        var key = await _repo.GetApiKeyByIdAsync(request.KeyId, ct);
        if (key is null)
        {
            _logger.LogWarning("API key {KeyId} not found", request.KeyId);
            return EmptyResponse.Value;
        }

        await _repo.DeleteApiKeyAsync(request.KeyId, ct);
        _logger.LogInformation("API key {KeyId} deleted successfully", request.KeyId);

        return EmptyResponse.Value;
    }
}
