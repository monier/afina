using Afina.Data.Entities;

namespace Afina.Modules.Vault.Endpoints.ListVaultItems;

public record VaultItemDto(
    Guid Id,
    VaultType Type,
    string Data,
    string? Metadata,
    DateTimeOffset CreatedAtUtc,
    Guid CreatedBy
);

public record ListVaultItemsResponse(
    List<VaultItemDto> Items
);
