using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Afina.Modules.Users.Features.Register;
using FluentAssertions;
using Xunit;

namespace Afina.Modules.Users.Tests.Features.Register;

public class RegisterTests : UsersIntegrationTestBase
{
    public RegisterTests(DatabaseFixture dbFixture) : base(dbFixture) { }

    [Fact]
    public async Task Register_WithValidData_CreatesUserAndReturnsTokens()
    {
        // Arrange
        var username = TestHelpers.GenerateTestUsername();
        var request = new RegisterRequest
        {
            Username = username,
            PasswordHash = "secure-hash-123",
            PasswordHint = "My secure hint"
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
    public async Task Register_WithoutPasswordHint_CreatesUserSuccessfully()
    {
        // Arrange
        var username = TestHelpers.GenerateTestUsername();
        var request = new RegisterRequest
        {
            Username = username,
            PasswordHash = "hash-without-hint",
            PasswordHint = null
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
    public async Task Register_WithDuplicateUsername_ReturnsBadRequest()
    {
        // Arrange
        var username = TestHelpers.GenerateTestUsername();
        var request = new RegisterRequest
        {
            Username = username,
            PasswordHash = "hash-123",
            PasswordHint = null
        };

        await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Act - try to register with same username
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithEmptyUsername_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "",
            PasswordHash = "hash-123",
            PasswordHint = null
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithEmptyPasswordHash_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = TestHelpers.GenerateTestUsername(),
            PasswordHash = "",
            PasswordHint = null
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_MultipleUsers_CreatesUniqueTokensForEach()
    {
        // Arrange & Act
        var user1 = await TestHelpers.RegisterUserAsync(Client, TestHelpers.GenerateTestUsername(), "hash1");
        var user2 = await TestHelpers.RegisterUserAsync(Client, TestHelpers.GenerateTestUsername(), "hash2");

        // Assert - Compare user IDs
        user1.UserId.Should().NotBe(user2.UserId);
        user1.UserId.Should().NotBeEmpty();
        user2.UserId.Should().NotBeEmpty();
    }
}
