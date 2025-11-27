using Afina.Data;
using Afina.Data.Entities;
using Afina.Modules.Tenant.Endpoints.CreateTenant;

namespace Afina.Modules.Tenant.Services;

public class TenantService
{
    private readonly AfinaDbContext _db;

    public TenantService(AfinaDbContext db)
    {
        _db = db;
    }

    public async Task<CreateTenantResponse> CreateTenantAsync(CreateTenantRequest request, Guid currentUserId, CancellationToken ct = default)
    {
        var tenantId = Guid.NewGuid();
        var encryptionId = Guid.NewGuid();

        var tenant = new Afina.Data.Entities.Tenant
        {
            Id = tenantId,
            Name = request.Name,
            Type = TenantType.Organization,
            EncryptionServiceUrl = request.EncryptionServiceUrl,
            EncryptionId = encryptionId,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = currentUserId
        };

        var membership = new TenantMembership
        {
            TenantId = tenantId,
            UserId = currentUserId,
            Role = TenantRole.TenantAdmin,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = currentUserId
        };

        _db.Tenants.Add(tenant);
        _db.TenantMemberships.Add(membership);

        await _db.SaveChangesAsync(ct);

        return new CreateTenantResponse(tenantId, tenant.Name, encryptionId);
    }
}
