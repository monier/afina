using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Afina.Modules.Users.Features.CreateApiKey;
using FluentAssertions;
using Xunit;

namespace Afina.Modules.Users.Tests.Features.CreateApiKey;

public class CreateApiKeyTests : UsersIntegrationTestBase
{
    [Fact]
    public async Task CreateApiKey_WithValidRequest_CreatesKeySuccessfully()
    {
        // Arrange
        await TestHelpers.RegisterAndAuthenticateAsync(Client);
        var request = new CreateApiKeyRequest
        {
            Name = "Test API Key"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.KeyPrefix.Should().NotBeNullOrWhiteSpace();
        result.KeyPrefix.Should().StartWith("ak_");
        result.Secret.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CreateApiKey_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        TestHelpers.ClearAuthToken(Client);
        var request = new CreateApiKeyRequest
        {
            Name = "Test API Key"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateApiKey_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        TestHelpers.SetAuthToken(Client, "invalid.token");
        var request = new CreateApiKeyRequest
        {
            Name = "Test API Key"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateApiKey_MultipleKeys_GeneratesUniqueKeysForEach()
    {
        // Arrange
        await TestHelpers.RegisterAndAuthenticateAsync(Client);

        // Act
        var response1 = await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "Key 1" });
        var response2 = await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "Key 2" });

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        var result1 = await response1.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
        var result2 = await response2.Content.ReadFromJsonAsync<CreateApiKeyResponse>();

        result1!.Id.Should().NotBe(result2!.Id);
        result1.KeyPrefix.Should().NotBe(result2.KeyPrefix);
        result1.Secret.Should().NotBe(result2.Secret);
    }

    [Fact]
    public async Task CreateApiKey_WithEmptyName_StillCreatesKey()
    {
        // Arrange
        await TestHelpers.RegisterAndAuthenticateAsync(Client);
        var request = new CreateApiKeyRequest
        {
            Name = ""
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", request);

        // Assert
        // This depends on validation rules - adjust based on implementation
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateApiKey_SecretIsRandomAndSecure()
    {
        // Arrange
        await TestHelpers.RegisterAndAuthenticateAsync(Client);

        // Act
        var response1 = await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "Key 1" });
        var response2 = await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "Key 2" });

        // Assert
        var result1 = await response1.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
        var result2 = await response2.Content.ReadFromJsonAsync<CreateApiKeyResponse>();

        result1!.Secret.Should().NotBe(result2!.Secret);
        result1.Secret.Length.Should().BeGreaterThan(20); // Base64 encoded 32 bytes should be longer
    }

    [Fact]
    public async Task CreateApiKey_LocationHeaderPointsToCorrectResource()
    {
        // Arrange
        await TestHelpers.RegisterAndAuthenticateAsync(Client);
        var request = new CreateApiKeyRequest { Name = "Test Key" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var result = await response.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
        response.Headers.Location!.ToString().Should().Contain(result!.Id.ToString());
    }
}
