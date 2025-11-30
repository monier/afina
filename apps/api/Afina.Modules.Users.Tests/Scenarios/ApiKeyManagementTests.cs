using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Afina.Modules.Users.Features.CreateApiKey;
using Afina.Modules.Users.Features.ListApiKeys;
using FluentAssertions;
using Xunit;

namespace Afina.Modules.Users.Tests.Scenarios;

/// <summary>
/// Scenarios testing API key management workflows
/// </summary>
public class ApiKeyManagementTests : UsersIntegrationTestBase
{
    [Fact]
    public async Task ApiKeyManagement_CreateListDeleteKeys_WorksCorrectly()
    {
        // Arrange
        await TestHelpers.RegisterAndAuthenticateAsync(Client);

        // Create multiple keys
        var key1Response = await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "Production" });
        var key2Response = await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "Staging" });
        var key3Response = await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "Development" });

        var key1 = await key1Response.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
        var key2 = await key2Response.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
        var key3 = await key3Response.Content.ReadFromJsonAsync<CreateApiKeyResponse>();

        // Verify all keys are listed
        var listResponse1 = await Client.GetAsync("/api/v1/users/me/api-keys");
        var list1 = await listResponse1.Content.ReadFromJsonAsync<ListApiKeysResponse>();
        list1!.Keys.Should().HaveCount(3);

        // Delete middle key
        await Client.DeleteAsync($"/api/v1/users/me/api-keys/{key2!.Id}");

        // Verify only 2 keys remain
        var listResponse2 = await Client.GetAsync("/api/v1/users/me/api-keys");
        var list2 = await listResponse2.Content.ReadFromJsonAsync<ListApiKeysResponse>();
        list2!.Keys.Should().HaveCount(2);

        // Delete remaining keys
        await Client.DeleteAsync($"/api/v1/users/me/api-keys/{key1!.Id}");
        await Client.DeleteAsync($"/api/v1/users/me/api-keys/{key3!.Id}");

        // Verify no keys remain
        var listResponse3 = await Client.GetAsync("/api/v1/users/me/api-keys");
        var list3 = await listResponse3.Content.ReadFromJsonAsync<ListApiKeysResponse>();
        list3!.Keys.Should().BeEmpty();
    }

    [Fact]
    public async Task ApiKeyManagement_KeysIsolatedBetweenUsers()
    {
        // Create first user with keys
        var user1Response = await TestHelpers.RegisterUserAsync(Client, TestHelpers.GenerateTestUsername(), "hash1");
        TestHelpers.SetAuthToken(Client, user1Response.Token);
        await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "User1-Key1" });
        await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "User1-Key2" });

        // Verify user1 sees 2 keys
        var user1Keys = await Client.GetAsync("/api/v1/users/me/api-keys");
        var user1List = await user1Keys.Content.ReadFromJsonAsync<ListApiKeysResponse>();
        user1List!.Keys.Should().HaveCount(2);

        // Create second user with different keys
        TestHelpers.ClearAuthToken(Client);
        var user2Response = await TestHelpers.RegisterUserAsync(Client, TestHelpers.GenerateTestUsername(), "hash2");
        TestHelpers.SetAuthToken(Client, user2Response.Token);
        await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "User2-Key1" });

        // Verify user2 sees only their own key
        var user2Keys = await Client.GetAsync("/api/v1/users/me/api-keys");
        var user2List = await user2Keys.Content.ReadFromJsonAsync<ListApiKeysResponse>();
        user2List!.Keys.Should().HaveCount(1);

        // Switch back to user1 and verify their keys are unchanged
        TestHelpers.SetAuthToken(Client, user1Response.Token);
        var user1KeysAgain = await Client.GetAsync("/api/v1/users/me/api-keys");
        var user1ListAgain = await user1KeysAgain.Content.ReadFromJsonAsync<ListApiKeysResponse>();
        user1ListAgain!.Keys.Should().HaveCount(2);
    }
}
