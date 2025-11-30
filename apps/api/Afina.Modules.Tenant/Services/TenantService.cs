using Afina.Data;
using Afina.Data.Entities;
using Afina.Modules.Tenant.Endpoints.CreateTenant;
using Microsoft.Extensions.Logging;

namespace Afina.Modules.Tenant.Services;

public class TenantService
{
    private readonly AfinaDbContext _db;
    private readonly ILogger<TenantService> _logger;

    public TenantService(AfinaDbContext db, ILogger<TenantService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<CreateTenantResponse> CreateTenantAsync(CreateTenantRequest request, Guid currentUserId, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating tenant {TenantName} for user {UserId}", request.Name, currentUserId);

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

        _logger.LogInformation(
            "Tenant {TenantId} created successfully with name {TenantName} for user {UserId}",
            tenantId,
            request.Name,
            currentUserId
        );

        return new CreateTenantResponse(tenantId, tenant.Name, encryptionId);
    }
}
