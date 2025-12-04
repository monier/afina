using System.Threading;
using System.Threading.Tasks;
using Afina.Data;
using Afina.Data.Entities; // assuming User entity exists
using Microsoft.EntityFrameworkCore;

namespace Afina.Modules.Users.Shared.Persistence;

public sealed class UsersRepository : IUserRepository
{
    private readonly AfinaDbContext _db;

    public UsersRepository(AfinaDbContext db) => _db = db;

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct)
        => _db.Set<User>().FirstOrDefaultAsync(u => u.Username == username, ct);

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken ct)
        => _db.Set<User>().FirstOrDefaultAsync(u => u.Id == userId, ct);

    public Task<bool> VerifyAuthHashAsync(User user, string authHash, CancellationToken ct)
    {
        // Verify password using BCrypt
        var isValid = BCrypt.Net.BCrypt.Verify(authHash, user.PasswordHash);
        return Task.FromResult(isValid);
    }

    public async Task<User> CreateUserAsync(string username, string passwordHash, string? passwordHint, CancellationToken ct)
    {
        // Ensure race-safe first-admin assignment using a SERIALIZABLE transaction
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);

        var existingUsersCount = await _db.Set<User>().CountAsync(ct);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = passwordHash,
            PasswordHint = passwordHint,
            SystemRole = existingUsersCount == 0 ? SystemRole.Admin : SystemRole.Member,
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Set<User>().Add(user);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return user;
    }

    public async Task DeleteUserAsync(User user, CancellationToken ct)
    {
        _db.Set<User>().Remove(user);
        await _db.SaveChangesAsync(ct);
    }
}
