using Afina.Data;
using Afina.Data.Entities;
using Afina.Modules.Vault.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Afina.Modules.Vault.Services;

public class VaultService
{
    private readonly AfinaDbContext _db;

    public VaultService(AfinaDbContext db)
    {
        _db = db;
    }

    public async Task<CreateVaultItemResponse> CreateVaultItemAsync(Guid tenantId, CreateVaultItemRequest request, Guid currentUserId, CancellationToken ct = default)
    {
        // TODO: Encrypt the data using the Encryption Service
        // For now, we'll just store it as plaintext (NOT SECURE - DEMO ONLY)
        var cipherText = request.Data;
        var encryptionVersionId = Guid.NewGuid();

        var vaultItem = new Afina.Data.Entities.Vault
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Type = request.Type,
            CipherText = cipherText,
            EncryptionId = Guid.NewGuid(),
            EncryptionVersionId = encryptionVersionId,
            Metadata = request.Metadata,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = currentUserId
        };

        _db.VaultItems.Add(vaultItem);
        await _db.SaveChangesAsync(ct);

        return new CreateVaultItemResponse(vaultItem.Id, vaultItem.Type, vaultItem.Metadata, vaultItem.CreatedAtUtc);
    }

    public async Task<ListVaultItemsResponse> ListVaultItemsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var items = await _db.VaultItems
            .Where(v => v.TenantId == tenantId)
            .Select(v => new VaultItemDto(
                v.Id,
                v.Type,
                v.CipherText, // TODO: Decrypt this using the Encryption Service
                v.Metadata,
                v.CreatedAtUtc,
                v.CreatedBy
            ))
            .ToListAsync(ct);

        return new ListVaultItemsResponse(items);
    }
}
