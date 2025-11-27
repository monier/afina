namespace Afina.Modules.Tenant.Endpoints.CreateTenant;

public record CreateTenantResponse(
    Guid TenantId,
    string Name,
    Guid EncryptionId
);
