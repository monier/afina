using System;

namespace Afina.Data.Entities;

public enum VaultType
{
    Credential,
    Document,
    Note,
    Media
}

public class Vault : EntityBase
{
    public Guid TenantId { get; set; }
    public VaultType Type { get; set; }
    public string CipherText { get; set; } = string.Empty;
    public Guid EncryptionId { get; set; }
    public Guid EncryptionVersionId { get; set; }
    public string? EncryptionMetadata { get; set; } // JSONB
    public string? Metadata { get; set; } // JSONB

    public Tenant Tenant { get; set; } = null!;
}
