using Afina.Data;
using Afina.Data.Entities;
using Afina.Modules.Vault.Endpoints.CreateVaultItem;
using Afina.Modules.Vault.Endpoints.ListVaultItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Afina.Modules.Vault.Services;

public class VaultService
{
    private readonly AfinaDbContext _db;
    private readonly ILogger<VaultService> _logger;

    public VaultService(AfinaDbContext db, ILogger<VaultService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<CreateVaultItemResponse> CreateVaultItemAsync(Guid tenantId, CreateVaultItemRequest request, Guid currentUserId, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Creating vault item of type {ItemType} for tenant {TenantId} by user {UserId}",
            request.Type,
            tenantId,
            currentUserId
        );

        // TODO: Encrypt the data using the Encryption Service
        // For now, we'll just store it as plaintext (NOT SECURE - DEMO ONLY)
        _logger.LogWarning(
            "Vault item data is not encrypted (DEMO ONLY - implement encryption service)"
        );

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

        _logger.LogInformation(
            "Vault item {ItemId} created successfully for tenant {TenantId}",
            vaultItem.Id,
            tenantId
        );

        return new CreateVaultItemResponse(vaultItem.Id, vaultItem.Type, vaultItem.Metadata, vaultItem.CreatedAtUtc);
    }

    public async Task<ListVaultItemsResponse> ListVaultItemsAsync(Guid tenantId, CancellationToken ct = default)
    {
        _logger.LogDebug("Listing vault items for tenant {TenantId}", tenantId);

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

        _logger.LogDebug(
            "Found {ItemCount} vault items for tenant {TenantId}",
            items.Count,
            tenantId
        );

        return new ListVaultItemsResponse(items);
    }
}
