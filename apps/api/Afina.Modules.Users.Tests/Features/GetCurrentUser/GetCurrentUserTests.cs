using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Afina.Modules.Users.Features.GetCurrentUser;
using FluentAssertions;
using Xunit;

namespace Afina.Modules.Users.Tests.Features.GetCurrentUser;

public class GetCurrentUserTests : UsersIntegrationTestBase
{
    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsUserProfile()
    {
        // Arrange
        var (registerResponse, username, _) = await TestHelpers.RegisterAndAuthenticateAsync(Client);

        // Act
        var response = await Client.GetAsync("/api/v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetCurrentUserResponse>();
        result.Should().NotBeNull();

        // Get user ID from registerResponse.User
        using var jsonDoc = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(registerResponse.User));
        var expectedUserId = jsonDoc.RootElement.GetProperty("id").GetGuid();

        result!.Id.Should().Be(expectedUserId);
        result.Username.Should().Be(username);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        TestHelpers.ClearAuthToken(Client);

        // Act
        var response = await Client.GetAsync("/api/v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        TestHelpers.SetAuthToken(Client, "invalid.token.here");

        // Act
        var response = await Client.GetAsync("/api/v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_AfterRefreshToken_ReturnsCorrectUser()
    {
        // Arrange
        var (registerResponse, username, _) = await TestHelpers.RegisterAndAuthenticateAsync(Client);

        // Refresh the token
        var refreshRequest = new Afina.Modules.Users.Features.RefreshToken.RefreshTokenRequest
        {
            RefreshToken = registerResponse.RefreshToken
        };
        var refreshResponse = await Client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);
        var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<Afina.Modules.Users.Features.RefreshToken.RefreshTokenResponse>();

        // Set the new token
        TestHelpers.SetAuthToken(Client, refreshResult!.Token);

        // Act
        var response = await Client.GetAsync("/api/v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetCurrentUserResponse>();
        result.Should().NotBeNull();
        result!.Username.Should().Be(username);
    }

    [Fact]
    public async Task GetCurrentUser_MultipleCallsWithSameToken_ReturnsConsistentData()
    {
        // Arrange
        await TestHelpers.RegisterAndAuthenticateAsync(Client);

        // Act
        var response1 = await Client.GetAsync("/api/v1/users/me");
        var response2 = await Client.GetAsync("/api/v1/users/me");

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        var result1 = await response1.Content.ReadFromJsonAsync<GetCurrentUserResponse>();
        var result2 = await response2.Content.ReadFromJsonAsync<GetCurrentUserResponse>();

        result1.Should().BeEquivalentTo(result2);
    }
}
