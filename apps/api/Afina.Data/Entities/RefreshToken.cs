using System;

namespace Afina.Data.Entities;

public class RefreshToken : EntityBase
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }

    public User User { get; set; } = null!;
}
