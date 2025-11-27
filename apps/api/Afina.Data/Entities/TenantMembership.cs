using System;

namespace Afina.Data.Entities;

public enum TenantRole
{
    TenantAdmin,
    TenantMember
}

public class TenantMembership : EntityBase
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public TenantRole Role { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public User User { get; set; } = null!;
}
