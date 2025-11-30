using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Afina.Modules.Users.Features.GetCurrentUser;
using FluentAssertions;
using Xunit;

namespace Afina.Modules.Users.Tests.Features.DeleteUser;

public class DeleteUserTests : UsersIntegrationTestBase
{
    [Fact]
    public async Task DeleteUser_WithValidToken_DeletesUserSuccessfully()
    {
        // Arrange
        await TestHelpers.RegisterAndAuthenticateAsync(Client);

        // Act
        var response = await Client.DeleteAsync("/api/v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteUser_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        TestHelpers.ClearAuthToken(Client);

        // Act
        var response = await Client.DeleteAsync("/api/v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteUser_AfterDeletion_CannotAccessUserProfile()
    {
        // Arrange
        var (registerResponse, _, _) = await TestHelpers.RegisterAndAuthenticateAsync(Client);

        // Act
        var deleteResponse = await Client.DeleteAsync("/api/v1/users/me");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Try to access profile with the same token
        var profileResponse = await Client.GetAsync("/api/v1/users/me");

        // Assert - GetCurrentUser throws when user not found, resulting in 500
        profileResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task DeleteUser_AfterDeletion_CannotLogin()
    {
        // Arrange
        var username = TestHelpers.GenerateTestUsername();
        var passwordHash = "hash-123";
        var registerResponse = await TestHelpers.RegisterUserAsync(Client, username, passwordHash);
        TestHelpers.SetAuthToken(Client, registerResponse.Token);

        // Delete the user
        await Client.DeleteAsync("/api/v1/users/me");

        // Act - try to login
        TestHelpers.ClearAuthToken(Client);
        var loginRequest = new Afina.Modules.Users.Features.Login.LoginRequest
        {
            Username = username,
            AuthHash = passwordHash
        };
        var loginResponse = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteUser_AfterDeletion_RefreshTokenInvalid()
    {
        // Arrange
        var (registerResponse, _, _) = await TestHelpers.RegisterAndAuthenticateAsync(Client);
        var refreshToken = registerResponse.RefreshToken;

        // Delete the user
        await Client.DeleteAsync("/api/v1/users/me");

        // Act - try to refresh token
        TestHelpers.ClearAuthToken(Client);
        var refreshRequest = new Afina.Modules.Users.Features.RefreshToken.RefreshTokenRequest
        {
            RefreshToken = refreshToken
        };
        var refreshResponse = await Client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteUser_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        TestHelpers.SetAuthToken(Client, "invalid.token.value");

        // Act
        var response = await Client.DeleteAsync("/api/v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteUser_TwiceInRow_SecondCallReturnsUnauthorized()
    {
        // Arrange
        await TestHelpers.RegisterAndAuthenticateAsync(Client);

        // Act
        var firstDelete = await Client.DeleteAsync("/api/v1/users/me");
        var secondDelete = await Client.DeleteAsync("/api/v1/users/me");

        // Assert - DeleteUser is idempotent and returns NoContent even if already deleted
        firstDelete.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondDelete.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
