using System;

namespace Afina.Data.Entities;

public enum SystemRole
{
    Admin,
    Member
}

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PasswordHint { get; set; }
    public SystemRole SystemRole { get; set; }
    public Guid IndividualTenantId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? UpdatedBy { get; set; }
}
