using System;

namespace Afina.Data.Entities;

public enum SystemRole
{
    Admin,
    Member
}

public class User : EntityBase
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PasswordHint { get; set; }
    public SystemRole SystemRole { get; set; }
    public Guid IndividualTenantId { get; set; }
}
