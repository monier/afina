using Afina.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Afina.Data;

public class AfinaDbContext : DbContext
{
    private static string? _migrationsSchema;

    public AfinaDbContext(DbContextOptions<AfinaDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Sets the schema name for migrations history table.
    /// This should be called before migrations are run.
    /// </summary>
    public static void SetMigrationsSchema(string schema)
    {
        _migrationsSchema = schema;
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Configure migrations history table to use the specified schema
        // If no schema is set, migrations will use the default search_path
        if (!string.IsNullOrEmpty(_migrationsSchema))
        {
            optionsBuilder.UseNpgsql(b =>
                b.MigrationsHistoryTable("__EFMigrationsHistory", _migrationsSchema));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Don't use HasDefaultSchema - instead use search_path via connection string
        // This prevents model snapshot from capturing the schema

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
