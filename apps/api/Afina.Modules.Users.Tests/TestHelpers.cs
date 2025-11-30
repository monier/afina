using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Afina.Modules.Users.Features.Register;
using Afina.Modules.Users.Features.Login;

namespace Afina.Modules.Users.Tests;

/// <summary>
/// Helper methods for test setup and common operations
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Registers a new user and returns the registration response
    /// </summary>
    public static async Task<RegisterResponse> RegisterUserAsync(
        HttpClient client,
        string username,
        string passwordHash,
        string? passwordHint = null)
    {
        var request = new RegisterRequest
        {
            Username = username,
            PasswordHash = passwordHash,
            PasswordHint = passwordHint
        };

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        return result ?? throw new InvalidOperationException("Registration failed");
    }

    /// <summary>
    /// Logs in an existing user and returns the login response
    /// </summary>
    public static async Task<LoginResponse> LoginUserAsync(
        HttpClient client,
        string username,
        string authHash)
    {
        var request = new LoginRequest
        {
            Username = username,
            AuthHash = authHash
        };

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result ?? throw new InvalidOperationException("Login failed");
    }

    /// <summary>
    /// Sets the Authorization header with the provided bearer token
    /// </summary>
    public static void SetAuthToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Clears the Authorization header
    /// </summary>
    public static void ClearAuthToken(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Generates a unique test username
    /// </summary>
    public static string GenerateTestUsername(string prefix = "test")
    {
        return $"{prefix}_{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Registers a user and sets the auth token on the client
    /// </summary>
    public static async Task<(RegisterResponse Response, string Username, string PasswordHash)> RegisterAndAuthenticateAsync(
        HttpClient client,
        string? username = null,
        string? passwordHash = null)
    {
        username ??= GenerateTestUsername();
        passwordHash ??= $"hash_{Guid.NewGuid():N}";

        var response = await RegisterUserAsync(client, username, passwordHash);
        SetAuthToken(client, response.Token);

        return (response, username, passwordHash);
    }
}
