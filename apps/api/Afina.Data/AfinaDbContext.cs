using Afina.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Afina.Data;

public class AfinaDbContext : DbContext
{
    public AfinaDbContext(DbContextOptions<AfinaDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantMembership> TenantMemberships { get; set; }
    public DbSet<Vault> VaultItems { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Encryption> Encryptions { get; set; }
    public DbSet<EncryptionKeyVersion> EncryptionKeyVersions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // TenantMembership
        modelBuilder.Entity<TenantMembership>(entity =>
        {
            entity.HasKey(e => new { e.TenantId, e.UserId });
            entity.HasOne(d => d.Tenant)
                .WithMany(p => p.Memberships)
                .HasForeignKey(d => d.TenantId);
        });

        // ApiKeyTenantAccess
        modelBuilder.Entity<ApiKeyTenantAccess>(entity =>
        {
            entity.HasKey(e => new { e.ApiKeyId, e.TenantId });
            entity.HasOne(d => d.ApiKey)
                .WithMany(p => p.TenantAccess)
                .HasForeignKey(d => d.ApiKeyId);
        });

        // Vault
        modelBuilder.Entity<Vault>(entity =>
        {
            entity.HasOne(d => d.Tenant)
                .WithMany(p => p.VaultItems)
                .HasForeignKey(d => d.TenantId);
        });
    }
}
