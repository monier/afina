using System;
using System.Collections.Generic;

namespace Afina.Data.Entities;

public class ApiKey
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public string SecretHash { get; set; } = string.Empty;
    public string Scopes { get; set; } = string.Empty; // JSONB
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? UpdatedBy { get; set; }

    public User User { get; set; } = null!;
    public ICollection<ApiKeyTenantAccess> TenantAccess { get; set; } = new List<ApiKeyTenantAccess>();
}

public class ApiKeyTenantAccess
{
    public Guid ApiKeyId { get; set; }
    public Guid TenantId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }

    public ApiKey ApiKey { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
