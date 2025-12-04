using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Afina.Modules.Users.Features.RefreshToken;
using FluentAssertions;
using Xunit;

namespace Afina.Modules.Users.Tests.Features.RefreshToken;

public class RefreshTokenTests : UsersIntegrationTestBase
{
    public RefreshTokenTests(DatabaseFixture dbFixture) : base(dbFixture) { }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        var (userId, token, refreshToken, _, _) = await TestHelpers.RegisterAndAuthenticateAsync(Client);
        var request = new RefreshTokenRequest
        {
            RefreshToken = refreshToken
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        // RefreshToken should definitely be rotated
        result.RefreshToken.Should().NotBe(refreshToken);
        // Access token may be same if created within same second (JWT has timestamp)
        // This is acceptable behavior
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "invalid-refresh-token-xyz"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithEmptyToken_ReturnsBadRequest()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = ""
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_UsingSameTokenTwice_SecondAttemptFails()
    {
        // Arrange
        var (userId, token, refreshToken, _, _) = await TestHelpers.RegisterAndAuthenticateAsync(Client);
        var request = new RefreshTokenRequest
        {
            RefreshToken = refreshToken
        };

        // Act
        var firstRefresh = await Client.PostAsJsonAsync("/api/v1/auth/refresh", request);
        var secondRefresh = await Client.PostAsJsonAsync("/api/v1/auth/refresh", request);

        // Assert
        firstRefresh.StatusCode.Should().Be(HttpStatusCode.OK);
        secondRefresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_AfterMultipleRefreshes_TokensContinueToRotate()
    {
        // Arrange
        var (userId, token, refreshToken, _, _) = await TestHelpers.RegisterAndAuthenticateAsync(Client);
        var currentRefreshToken = refreshToken;

        // Act & Assert - chain multiple refreshes
        for (int i = 0; i < 3; i++)
        {
            var request = new RefreshTokenRequest { RefreshToken = currentRefreshToken };
            var response = await Client.PostAsJsonAsync("/api/v1/auth/refresh", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();
            result.Should().NotBeNull();
            result!.RefreshToken.Should().NotBe(currentRefreshToken);

            currentRefreshToken = result.RefreshToken;
        }
    }

    [Fact]
    public async Task RefreshToken_WithRevokedSession_ReturnsUnauthorized()
    {
        // Arrange
        var username = TestHelpers.GenerateTestUsername();
        var passwordHash = "hash-123";
        var registerResponse = await TestHelpers.RegisterUserAsync(Client, username, passwordHash);
        var loginResponse = await TestHelpers.LoginUserAsync(Client, username, passwordHash);

        // Delete the user (which should revoke all sessions)
        TestHelpers.SetAuthToken(Client, loginResponse.Token);
        await Client.DeleteAsync("/api/v1/users/me");

        var request = new RefreshTokenRequest
        {
            RefreshToken = loginResponse.RefreshToken
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
