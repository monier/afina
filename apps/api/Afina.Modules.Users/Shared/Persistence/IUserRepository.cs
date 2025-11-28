using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Data.Entities; // assuming entities exist in Afina.Data

namespace Afina.Modules.Users.Shared.Persistence;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct);
    Task<User?> GetByIdAsync(Guid userId, CancellationToken ct);
    Task<bool> VerifyAuthHashAsync(User user, string authHash, CancellationToken ct);
    Task<User> CreateUserAsync(string username, string passwordHash, string? passwordHint, CancellationToken ct);
    Task DeleteUserAsync(User user, CancellationToken ct);
}
