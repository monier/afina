using System;
using System.Collections.Generic;

namespace Afina.Data.Entities;

public enum TenantType
{
    Individual,
    Organization
}

public class Tenant : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public TenantType Type { get; set; }
    public string? EncryptionServiceUrl { get; set; }
    public string? EncryptionConfig { get; set; } // JSONB
    public Guid EncryptionId { get; set; }

    public ICollection<TenantMembership> Memberships { get; set; } = new List<TenantMembership>();
    public ICollection<Vault> VaultItems { get; set; } = new List<Vault>();
}
