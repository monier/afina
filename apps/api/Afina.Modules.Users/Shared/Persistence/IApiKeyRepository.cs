using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Afina.Data.Entities;

namespace Afina.Modules.Users.Shared.Persistence;

public interface IApiKeyRepository
{
    Task<List<ApiKey>> GetUserApiKeysAsync(Guid userId, CancellationToken ct);
    Task<ApiKey> CreateApiKeyAsync(Guid userId, string name, string keyPrefix, string secretHash, string scopes, DateTime? expiresAt, CancellationToken ct);
    Task<ApiKey?> GetApiKeyByIdAsync(Guid keyId, CancellationToken ct);
    Task DeleteApiKeyAsync(Guid keyId, CancellationToken ct);
}
