using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Afina.Data;
using Afina.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Afina.Modules.Users.Shared.Persistence;

public sealed class UserSessionsRepository : IUserSessionsRepository
{
    private readonly AfinaDbContext _db;

    public UserSessionsRepository(AfinaDbContext db) => _db = db;

    public async Task<string> CreateSessionAsync(Guid userId, string refreshToken, CancellationToken ct)
    {
        var session = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = userId
        };
        _db.Set<RefreshToken>().Add(session);
        await _db.SaveChangesAsync(ct);
        return refreshToken;
    }

    public async Task<Guid?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        var session = await _db.Set<RefreshToken>()
            .FirstOrDefaultAsync(s => s.Token == refreshToken && s.ExpiresAt > DateTime.UtcNow, ct);
        return session?.UserId;
    }

    public async Task RevokeSessionAsync(string refreshToken, CancellationToken ct)
    {
        var session = await _db.Set<RefreshToken>()
            .FirstOrDefaultAsync(s => s.Token == refreshToken, ct);
        if (session != null)
        {
            _db.Set<RefreshToken>().Remove(session);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task RevokeAllUserSessionsAsync(Guid userId, CancellationToken ct)
    {
        var sessions = await _db.Set<RefreshToken>()
            .Where(s => s.UserId == userId)
            .ToListAsync(ct);

        _db.Set<RefreshToken>().RemoveRange(sessions);
        await _db.SaveChangesAsync(ct);
    }
}
