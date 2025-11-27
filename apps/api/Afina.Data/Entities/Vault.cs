using System;

namespace Afina.Data.Entities;

public enum VaultType
{
    Credential,
    Document,
    Note,
    Media
}

public class Vault
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public VaultType Type { get; set; }
    public string CipherText { get; set; } = string.Empty;
    public Guid EncryptionId { get; set; }
    public Guid EncryptionVersionId { get; set; }
    public string? EncryptionMetadata { get; set; } // JSONB
    public string? Metadata { get; set; } // JSONB
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? UpdatedBy { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
