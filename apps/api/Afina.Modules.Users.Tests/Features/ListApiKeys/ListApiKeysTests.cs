using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Afina.Modules.Users.Features.CreateApiKey;
using Afina.Modules.Users.Features.ListApiKeys;
using FluentAssertions;
using Xunit;

namespace Afina.Modules.Users.Tests.Features.ListApiKeys;

public class ListApiKeysTests : UsersIntegrationTestBase
{
    [Fact]
    public async Task ListApiKeys_WithNoKeys_ReturnsEmptyList()
    {
        // Arrange
        await TestHelpers.RegisterAndAuthenticateAsync(Client);

        // Act
        var response = await Client.GetAsync("/api/v1/users/me/api-keys");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ListApiKeysResponse>();
        result.Should().NotBeNull();
        result!.Keys.Should().BeEmpty();
    }

    [Fact]
    public async Task ListApiKeys_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        TestHelpers.ClearAuthToken(Client);

        // Act
        var response = await Client.GetAsync("/api/v1/users/me/api-keys");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListApiKeys_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        TestHelpers.SetAuthToken(Client, "invalid.token");

        // Act
        var response = await Client.GetAsync("/api/v1/users/me/api-keys");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListApiKeys_AfterCreatingOne_ReturnsOneKey()
    {
        // Arrange
        await TestHelpers.RegisterAndAuthenticateAsync(Client);
        var createRequest = new CreateApiKeyRequest { Name = "Test Key" };
        var createResponse = await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", createRequest);
        var createdKey = await createResponse.Content.ReadFromJsonAsync<CreateApiKeyResponse>();

        // Act
        var response = await Client.GetAsync("/api/v1/users/me/api-keys");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ListApiKeysResponse>();
        result.Should().NotBeNull();
        result!.Keys.Should().HaveCount(1);
    }

    [Fact]
    public async Task ListApiKeys_AfterCreatingMultiple_ReturnsAllKeys()
    {
        // Arrange
        await TestHelpers.RegisterAndAuthenticateAsync(Client);

        await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "Key 1" });
        await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "Key 2" });
        await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "Key 3" });

        // Act
        var response = await Client.GetAsync("/api/v1/users/me/api-keys");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ListApiKeysResponse>();
        result.Should().NotBeNull();
        result!.Keys.Should().HaveCount(3);
    }

    [Fact]
    public async Task ListApiKeys_DoesNotExposeSecrets()
    {
        // Arrange
        await TestHelpers.RegisterAndAuthenticateAsync(Client);
        var createResponse = await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "Test Key" });
        var createdKey = await createResponse.Content.ReadFromJsonAsync<CreateApiKeyResponse>();

        // Act
        var response = await Client.GetAsync("/api/v1/users/me/api-keys");

        // Assert
        var result = await response.Content.ReadFromJsonAsync<ListApiKeysResponse>();
        result.Should().NotBeNull();
        result!.Keys.Should().HaveCount(1);

        var jsonString = await response.Content.ReadAsStringAsync();
        jsonString.Should().NotContain(createdKey!.Secret);
    }

    [Fact]
    public async Task ListApiKeys_OnlyReturnsKeysForCurrentUser()
    {
        // Arrange - create first user with keys
        var user1Username = TestHelpers.GenerateTestUsername();
        var user1Response = await TestHelpers.RegisterUserAsync(Client, user1Username, "hash1");
        TestHelpers.SetAuthToken(Client, user1Response.Token);
        await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "User1 Key" });

        // Create second user with different keys
        TestHelpers.ClearAuthToken(Client);
        var user2Username = TestHelpers.GenerateTestUsername();
        var user2Response = await TestHelpers.RegisterUserAsync(Client, user2Username, "hash2");
        TestHelpers.SetAuthToken(Client, user2Response.Token);
        await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "User2 Key 1" });
        await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "User2 Key 2" });

        // Act - list keys for user 2
        var response = await Client.GetAsync("/api/v1/users/me/api-keys");

        // Assert - should only see user 2's keys
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ListApiKeysResponse>();
        result.Should().NotBeNull();
        result!.Keys.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListApiKeys_AfterDeletingKey_ReturnsRemainingKeys()
    {
        // Arrange
        await TestHelpers.RegisterAndAuthenticateAsync(Client);

        var createResponse1 = await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "Key 1" });
        var createResponse2 = await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "Key 2" });

        var key1 = await createResponse1.Content.ReadFromJsonAsync<CreateApiKeyResponse>();

        // Delete first key
        await Client.DeleteAsync($"/api/v1/users/me/api-keys/{key1!.Id}");

        // Act
        var response = await Client.GetAsync("/api/v1/users/me/api-keys");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ListApiKeysResponse>();
        result.Should().NotBeNull();
        result!.Keys.Should().HaveCount(1);
    }
}
