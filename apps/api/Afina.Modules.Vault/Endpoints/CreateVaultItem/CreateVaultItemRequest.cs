using Afina.Data.Entities;

namespace Afina.Modules.Vault.Endpoints.CreateVaultItem;

public record CreateVaultItemRequest(
    VaultType Type,
    string Data,
    string? Metadata
);
