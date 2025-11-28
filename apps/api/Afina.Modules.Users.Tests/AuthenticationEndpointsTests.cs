using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Afina.Modules.Users.Features.Login;
using Afina.Modules.Users.Features.Register;
using Afina.Modules.Users.Features.RefreshToken;
using Afina.Modules.Users.Features.GetCurrentUser;
using FluentAssertions;
using Xunit;

namespace Afina.Modules.Users.Tests;

public class AuthenticationEndpointsTests : UsersIntegrationTestBase
{
    [Fact]
    public async Task Register_CreatesUserAndReturnsTokens()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            PasswordHash = "hashed-password-123",
            PasswordHint = "My test hint"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "duplicate",
            PasswordHash = "hashed-password",
            PasswordHint = null
        };

        await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Act - try to register again
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        // Arrange - register first
        var registerRequest = new RegisterRequest
        {
            Username = "logintest",
            PasswordHash = "test-hash",
            PasswordHint = null
        };
        await Client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Username = "logintest",
            AuthHash = "test-hash"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "nonexistent",
            AuthHash = "wrong"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
    {
        // Arrange - register and get tokens
        var registerRequest = new RegisterRequest
        {
            Username = "refreshtest",
            PasswordHash = "test-hash",
            PasswordHint = null
        };
        var registerResponse = await Client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();

        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = registerResult!.RefreshToken
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBe(registerResult.RefreshToken); // Token should be rotated
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "invalid-token"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsUserProfile()
    {
        // Arrange - register and get token
        var registerRequest = new RegisterRequest
        {
            Username = "profiletest",
            PasswordHash = "test-hash",
            PasswordHint = "test hint"
        };
        var registerResponse = await Client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();

        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", registerResult!.Token);

        // Act
        var response = await Client.GetAsync("/api/v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetCurrentUserResponse>();
        result.Should().NotBeNull();
        result!.Username.Should().Be("profiletest");
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
