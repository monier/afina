using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;

namespace Afina.Modules.Users.Features.CreateApiKey;

public sealed class CreateApiKeyHandler : IRequestHandler<CreateApiKeyRequest, CreateApiKeyResponse>
{
    private readonly IApiKeyRepository _repo;
    public CreateApiKeyHandler(IApiKeyRepository repo) => _repo = repo;

    public async Task<CreateApiKeyResponse> HandleAsync(CreateApiKeyRequest request, CancellationToken ct)
    {
        var keyPrefix = $"ak_{Guid.NewGuid().ToString()[..8]}";
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var secretHash = BCrypt.Net.BCrypt.HashPassword(secret);
        var created = await _repo.CreateApiKeyAsync(request.UserId, request.Name, keyPrefix, secretHash, "*", null, ct);
        return new CreateApiKeyResponse { Id = created.Id, KeyPrefix = keyPrefix, Secret = secret };
    }
}
