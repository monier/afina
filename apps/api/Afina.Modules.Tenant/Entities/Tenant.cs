using System;
using System.Collections.Generic;

namespace Afina.Modules.Tenant.Entities
{
    public class Tenant
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TenantType Type { get; set; }
        public string? EncryptionServiceUrl { get; set; }
        public string? EncryptionConfig { get; set; }
        public Guid EncryptionId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public Guid UpdatedBy { get; set; }

        public ICollection<TenantMembership> Memberships { get; set; } = new List<TenantMembership>();
    }

    public enum TenantType
    {
        Individual,
        Organization
    }
}
