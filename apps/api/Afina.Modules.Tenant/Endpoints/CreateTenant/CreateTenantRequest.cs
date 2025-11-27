namespace Afina.Modules.Tenant.Endpoints.CreateTenant;

public record CreateTenantRequest(
    string Name,
    string? EncryptionServiceUrl
);
