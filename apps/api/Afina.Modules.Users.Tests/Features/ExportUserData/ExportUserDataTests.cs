using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Afina.Modules.Users.Features.ExportUserData;
using FluentAssertions;
using Xunit;

namespace Afina.Modules.Users.Tests.Features.ExportUserData;

public class ExportUserDataTests : UsersIntegrationTestBase
{
    public ExportUserDataTests(DatabaseFixture dbFixture) : base(dbFixture) { }

    [Fact]
    public async Task ExportUserData_WithValidToken_ReturnsUserData()
    {
        // Arrange
        var (userId, token, refreshToken, username, _) = await TestHelpers.RegisterAndAuthenticateAsync(Client);

        // Act
        var response = await Client.PostAsync("/api/v1/users/me/export", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ExportUserDataResponse>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNullOrWhiteSpace();

        // Verify the data contains user information
        var dataObj = JsonSerializer.Deserialize<JsonElement>(result.Data);
        dataObj.GetProperty("username").GetString().Should().Be(username);

        // Verify expected user ID
        dataObj.GetProperty("id").GetGuid().Should().Be(userId);
    }

    [Fact]
    public async Task ExportUserData_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        TestHelpers.ClearAuthToken(Client);

        // Act
        var response = await Client.PostAsync("/api/v1/users/me/export", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ExportUserData_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        TestHelpers.SetAuthToken(Client, "invalid.jwt.token");

        // Act
        var response = await Client.PostAsync("/api/v1/users/me/export", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ExportUserData_MultipleExports_ReturnsSameData()
    {
        // Arrange
        await TestHelpers.RegisterAndAuthenticateAsync(Client);

        // Act
        var response1 = await Client.PostAsync("/api/v1/users/me/export", null);
        var response2 = await Client.PostAsync("/api/v1/users/me/export", null);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        var result1 = await response1.Content.ReadFromJsonAsync<ExportUserDataResponse>();
        var result2 = await response2.Content.ReadFromJsonAsync<ExportUserDataResponse>();

        result1!.Data.Should().Be(result2!.Data);
    }

    [Fact]
    public async Task ExportUserData_ReturnsValidJson()
    {
        // Arrange
        await TestHelpers.RegisterAndAuthenticateAsync(Client);

        // Act
        var response = await Client.PostAsync("/api/v1/users/me/export", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ExportUserDataResponse>();

        // Verify it's valid JSON by attempting to deserialize
        var deserializeAction = () => JsonSerializer.Deserialize<JsonElement>(result!.Data);
        deserializeAction.Should().NotThrow();
    }
}
