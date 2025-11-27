using Afina.Data;
using Afina.Data.Entities;
using Afina.Modules.Identity.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Afina.Modules.Identity.Services;

public class AuthService
{
    private readonly AfinaDbContext _db;

    public AuthService(AfinaDbContext db)
    {
        _db = db;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        // Check if username already exists
        var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username, ct);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Username already exists");
        }

        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var encryptionId = Guid.NewGuid();

        // Create individual tenant
        var tenant = new Tenant
        {
            Id = tenantId,
            Name = $"{request.Username}'s Vault",
            Type = TenantType.Individual,
            EncryptionId = encryptionId,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = userId
        };

        // Create user
        var user = new User
        {
            Id = userId,
            Username = request.Username,
            PasswordHash = request.PasswordHash,
            PasswordHint = request.PasswordHint,
            SystemRole = SystemRole.Member,
            IndividualTenantId = tenantId,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = userId
        };

        // Create tenant membership
        var membership = new TenantMembership
        {
            TenantId = tenantId,
            UserId = userId,
            Role = TenantRole.TenantAdmin,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = userId
        };

        _db.Tenants.Add(tenant);
        _db.Users.Add(user);
        _db.TenantMemberships.Add(membership);

        await _db.SaveChangesAsync(ct);

        return new RegisterResponse(userId, tenantId, "User registered successfully");
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        // Find user by username
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username, ct);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // In a real implementation, you would verify the authHash against the stored passwordHash
        // For now, we'll just check if they match
        if (user.PasswordHash != request.AuthHash)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        return new LoginResponse(user.Id, user.Username, "Login successful");
    }
}
