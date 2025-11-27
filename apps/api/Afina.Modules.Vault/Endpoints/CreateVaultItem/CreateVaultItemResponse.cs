using Afina.Data.Entities;

namespace Afina.Modules.Vault.Endpoints.CreateVaultItem;

public record CreateVaultItemResponse(
    Guid Id,
    VaultType Type,
    string? Metadata,
    DateTimeOffset CreatedAtUtc
);
