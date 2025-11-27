using System;
using System.Collections.Generic;

namespace Afina.Modules.Encryption.Entities
{
    public class EncryptionContext
    {
        public Guid Id { get; set; } // Corresponds to Tenant.EncryptionId
        public string MasterKeyHash { get; set; } = string.Empty;
        public Guid CurrentSymmetricKeyId { get; set; }
        public Guid CurrentAsymmetricKeyId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public Guid UpdatedBy { get; set; }

        public ICollection<EncryptionKeyVersion> KeyVersions { get; set; } = new List<EncryptionKeyVersion>();
    }

    public class EncryptionKeyVersion
    {
        public Guid Id { get; set; }
        public Guid EncryptionContextId { get; set; }
        public EncryptionContext EncryptionContext { get; set; } = null!;
        public KeyType Type { get; set; }
        public string Algorithm { get; set; } = string.Empty;
        public string EncryptedKeyMaterial { get; set; } = string.Empty;
        public KeyStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedBy { get; set; }
    }

    public enum KeyType
    {
        Symmetric,
        Asymmetric
    }

    public enum KeyStatus
    {
        Active,
        Archived
    }
}
