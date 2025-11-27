using System;

namespace Afina.Modules.Tenant.Entities
{
    public class TenantMembership
    {
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
        public Guid UserId { get; set; }
        public TenantRole Role { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedBy { get; set; }
    }

    public enum TenantRole
    {
        TenantMember,
        TenantAdmin
    }
}
