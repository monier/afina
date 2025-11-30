using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;
using Microsoft.Extensions.Logging;

namespace Afina.Modules.Users.Features.CreateApiKey;

public sealed class CreateApiKeyHandler : IRequestHandler<CreateApiKeyRequest, CreateApiKeyResponse>
{
    private readonly IApiKeyRepository _repo;
    private readonly ILogger<CreateApiKeyHandler> _logger;

    public CreateApiKeyHandler(IApiKeyRepository repo, ILogger<CreateApiKeyHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<CreateApiKeyResponse> HandleAsync(CreateApiKeyRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Creating API key for user {UserId} with name {KeyName}", request.UserId, request.Name);

        var keyPrefix = $"ak_{Guid.NewGuid().ToString()[..8]}";
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var secretHash = BCrypt.Net.BCrypt.HashPassword(secret);
        var created = await _repo.CreateApiKeyAsync(request.UserId, request.Name, keyPrefix, secretHash, "*", null, ct);

        _logger.LogInformation("API key {KeyId} created successfully for user {UserId}", created.Id, request.UserId);

        return new CreateApiKeyResponse { Id = created.Id, KeyPrefix = keyPrefix, Secret = secret };
    }
}
