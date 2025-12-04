using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Core.Api;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Shared.Persistence;
using Afina.Modules.Users.Shared.Services;
using Microsoft.Extensions.Logging;

namespace Afina.Modules.Users.Features.Register;

public sealed class RegisterHandler : IRequestHandler<RegisterRequest, RegisterResponse>
{
    private readonly IUserRepository _users;
    private readonly ILogger<RegisterHandler> _logger;

    public RegisterHandler(
        IUserRepository users,
        ILogger<RegisterHandler> logger)
    {
        _users = users;
        _logger = logger;
    }

    public async Task<RegisterResponse> HandleAsync(RegisterRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Registration attempt for username: {Username}", request.Username);

        // Validate username
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            _logger.LogWarning("Registration failed: Username is required");
            throw new ApiException(
                ErrorCodes.USERNAME_REQUIRED,
                "Username is required.");
        }

        // Validate password hash
        if (string.IsNullOrWhiteSpace(request.PasswordHash))
        {
            _logger.LogWarning("Registration failed: Password is required");
            throw new ApiException(
                ErrorCodes.PASSWORD_REQUIRED,
                "Password is required.");
        }

        // Check username uniqueness
        var existing = await _users.GetByUsernameAsync(request.Username, ct);
        if (existing is not null)
        {
            _logger.LogWarning("Registration failed: Username {Username} already exists", request.Username);
            throw new ApiException(
                ErrorCodes.USERNAME_ALREADY_EXISTS,
                "Username already exists.");
        }

        // Hash the already hashed password (client-side hash is treated as the password)
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.PasswordHash);
        var user = await _users.CreateUserAsync(request.Username, hashedPassword, request.PasswordHint, ct);

        _logger.LogInformation("User {UserId} registered successfully with username {Username}", user.Id, user.Username);

        return new RegisterResponse
        {
            UserId = user.Id
        };
    }
}
