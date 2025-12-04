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

/// <summary>
/// Legacy authentication tests - kept for backward compatibility.
/// For comprehensive feature tests, see:
/// - Features/Register/RegisterTests.cs
/// - Features/Login/LoginTests.cs
/// - Features/RefreshToken/RefreshTokenTests.cs
/// - Features/GetCurrentUser/GetCurrentUserTests.cs
/// For scenario-based tests, see Scenarios/ folder.
/// </summary>
public class AuthenticationEndpointsTests : UsersIntegrationTestBase
{
    public AuthenticationEndpointsTests(DatabaseFixture dbFixture) : base(dbFixture) { }

    [Fact]
    public async Task Register_CreatesUserAndReturnsTokens()
    {
        // Arrange
        var username = TestHelpers.GenerateTestUsername();
        var request = new RegisterRequest
        {
            Username = username,
            PasswordHash = "hashed-password-123",
            PasswordHint = "My test hint"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        result.Should().NotBeNull();
        result!.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var username = TestHelpers.GenerateTestUsername();
        var passwordHash = "test-hash";
        await TestHelpers.RegisterUserAsync(Client, username, passwordHash);

        var loginRequest = new LoginRequest
        {
            Username = username,
            AuthHash = passwordHash
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
    public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        var (userId, token, refreshToken, _, _) = await TestHelpers.RegisterAndAuthenticateAsync(Client);
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = refreshToken
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBe(refreshToken);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsUserProfile()
    {
        // Arrange
        var (userId, token, refreshToken, username, _) = await TestHelpers.RegisterAndAuthenticateAsync(Client);

        // Act
        var response = await Client.GetAsync("/api/v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetCurrentUserResponse>();
        result.Should().NotBeNull();
        result!.Username.Should().Be(username);
    }
}
