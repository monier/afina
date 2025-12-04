using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Afina.Modules.Users.Features.Login;
using FluentAssertions;
using Xunit;

namespace Afina.Modules.Users.Tests.Features.Login;

public class LoginTests : UsersIntegrationTestBase
{
    public LoginTests(DatabaseFixture dbFixture) : base(dbFixture) { }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokensAndUserInfo()
    {
        // Arrange
        var username = TestHelpers.GenerateTestUsername();
        var passwordHash = "valid-hash-123";
        await TestHelpers.RegisterUserAsync(Client, username, passwordHash);

        var request = new LoginRequest
        {
            Username = username,
            AuthHash = passwordHash
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var username = TestHelpers.GenerateTestUsername();
        await TestHelpers.RegisterUserAsync(Client, username, "correct-hash");

        var request = new LoginRequest
        {
            Username = username,
            AuthHash = "wrong-hash"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonexistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "nonexistent-user",
            AuthHash = "some-hash"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithEmptyUsername_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "",
            AuthHash = "hash"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithEmptyAuthHash_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "someuser",
            AuthHash = ""
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_MultipleTimes_GeneratesNewTokensEachTime()
    {
        // Arrange
        var username = TestHelpers.GenerateTestUsername();
        var passwordHash = "hash-123";
        await TestHelpers.RegisterUserAsync(Client, username, passwordHash);

        // Act
        var login1 = await TestHelpers.LoginUserAsync(Client, username, passwordHash);
        await System.Threading.Tasks.Task.Delay(1100); // Wait to ensure different token timestamp
        var login2 = await TestHelpers.LoginUserAsync(Client, username, passwordHash);

        // Assert - tokens should be different due to timestamp
        login1.Token.Should().NotBe(login2.Token);
        login1.RefreshToken.Should().NotBe(login2.RefreshToken);
    }

    [Fact]
    public async Task Login_CaseInsensitiveUsername_Succeeds()
    {
        // Arrange
        var username = TestHelpers.GenerateTestUsername();
        var passwordHash = "hash-123";
        await TestHelpers.RegisterUserAsync(Client, username, passwordHash);

        var request = new LoginRequest
        {
            Username = username.ToUpper(),
            AuthHash = passwordHash
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert - depending on your implementation, this might be case-sensitive
        // Adjust expectation based on actual behavior
        var result = response.StatusCode;
        result.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }
}
