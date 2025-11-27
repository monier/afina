namespace Afina.Modules.Tenant.DTOs;

public record CreateTenantRequest(
    string Name,
    string? EncryptionServiceUrl
);

public record CreateTenantResponse(
    Guid TenantId,
    string Name,
    Guid EncryptionId
);
