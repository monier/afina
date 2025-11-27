using Afina.Data.Entities;

namespace Afina.Modules.Vault.DTOs;

public record CreateVaultItemRequest(
    VaultType Type,
    string Data,
    string? Metadata
);

public record CreateVaultItemResponse(
    Guid Id,
    VaultType Type,
    string? Metadata,
    DateTime CreatedAtUtc
);

public record VaultItemDto(
    Guid Id,
    VaultType Type,
    string Data,
    string? Metadata,
    DateTime CreatedAtUtc,
    Guid CreatedBy
);

public record ListVaultItemsResponse(
    List<VaultItemDto> Items
);
