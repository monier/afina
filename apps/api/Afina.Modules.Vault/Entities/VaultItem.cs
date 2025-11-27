using System;

namespace Afina.Modules.Vault.Entities
{
    public class VaultItem
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public VaultItemType Type { get; set; }
        public string CipherText { get; set; } = string.Empty;
        public Guid EncryptionId { get; set; }
        public Guid EncryptionVersionId { get; set; }
        public string? EncryptionMetadata { get; set; } // JSON
        public string? Metadata { get; set; } // JSON
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public Guid UpdatedBy { get; set; }
    }

    public enum VaultItemType
    {
        Credential,
        Document,
        Note,
        Media
    }
}
