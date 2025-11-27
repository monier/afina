using System;

namespace Afina.Data.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public string? Details { get; set; } // JSONB

    public User User { get; set; } = null!;
    public Tenant? Tenant { get; set; }
}
