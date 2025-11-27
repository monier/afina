using System;
using System.Collections.Generic;

namespace Afina.Data.Entities;

public class Encryption
{
    public Guid Id { get; set; }
    public string MasterKeyHash { get; set; } = string.Empty;
    public Guid CurrentSymmetricKeyId { get; set; }
    public Guid CurrentAsymmetricKeyId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? UpdatedBy { get; set; }

    public ICollection<EncryptionKeyVersion> KeyVersions { get; set; } = new List<EncryptionKeyVersion>();
}

public enum EncryptionKeyType
{
    Symmetric,
    Asymmetric
}

public enum EncryptionKeyStatus
{
    Active,
    Archived
}

public class EncryptionKeyVersion
{
    public Guid Id { get; set; }
    public Guid EncryptionId { get; set; }
    public EncryptionKeyType Type { get; set; }
    public string Algorithm { get; set; } = string.Empty;
    public string EncryptedKeyMaterial { get; set; } = string.Empty;
    public EncryptionKeyStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? UpdatedBy { get; set; }

    public Encryption Encryption { get; set; } = null!;
}
