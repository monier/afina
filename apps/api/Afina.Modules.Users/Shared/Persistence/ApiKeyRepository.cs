using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Afina.Data;
using Afina.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Afina.Modules.Users.Shared.Persistence;

public sealed class ApiKeyRepository : IApiKeyRepository
{
    private readonly AfinaDbContext _db;
    public ApiKeyRepository(AfinaDbContext db) => _db = db;

    public async Task<List<ApiKey>> GetUserApiKeysAsync(Guid userId, CancellationToken ct)
        => await _db.ApiKeys.Where(k => k.UserId == userId).ToListAsync(ct);

    public async Task<ApiKey> CreateApiKeyAsync(Guid userId, string name, string keyPrefix, string secretHash, string scopes, DateTime? expiresAt, CancellationToken ct)
    {
        var key = new ApiKey
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            KeyPrefix = keyPrefix,
            SecretHash = secretHash,
            Scopes = scopes,
            ExpiresAt = expiresAt,
            CreatedBy = userId,
            UpdatedBy = userId
        };
        _db.ApiKeys.Add(key);
        await _db.SaveChangesAsync(ct);
        return key;
    }

    public async Task<ApiKey?> GetApiKeyByIdAsync(Guid keyId, CancellationToken ct)
        => await _db.ApiKeys.FirstOrDefaultAsync(k => k.Id == keyId, ct);

    public async Task DeleteApiKeyAsync(Guid keyId, CancellationToken ct)
    {
        var key = await GetApiKeyByIdAsync(keyId, ct);
        if (key is not null)
        {
            _db.ApiKeys.Remove(key);
            await _db.SaveChangesAsync(ct);
        }
    }
}
