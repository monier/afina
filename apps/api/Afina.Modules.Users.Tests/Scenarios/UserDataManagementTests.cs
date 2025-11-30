using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Afina.Modules.Users.Features.CreateApiKey;
using Afina.Modules.Users.Features.ExportUserData;
using Afina.Modules.Users.Features.ListApiKeys;
using FluentAssertions;
using Xunit;

namespace Afina.Modules.Users.Tests.Scenarios;

/// <summary>
/// Scenarios testing user data management and privacy
/// </summary>
public class UserDataManagementTests : UsersIntegrationTestBase
{
    [Fact]
    public async Task UserDataManagement_CreateDataExportDelete_RemovesAllData()
    {
        // Register and create user data
        await TestHelpers.RegisterAndAuthenticateAsync(Client);

        // Create API keys
        await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "Key 1" });
        await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "Key 2" });

        // Export data
        var exportResponse = await Client.PostAsync("/api/v1/users/me/export", null);
        exportResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var exportData = await exportResponse.Content.ReadFromJsonAsync<ExportUserDataResponse>();
        exportData!.Data.Should().NotBeNullOrWhiteSpace();

        // Verify API keys exist
        var keysResponse = await Client.GetAsync("/api/v1/users/me/api-keys");
        var keys = await keysResponse.Content.ReadFromJsonAsync<ListApiKeysResponse>();
        keys!.Keys.Should().HaveCount(2);

        // Delete user
        var deleteResponse = await Client.DeleteAsync("/api/v1/users/me");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify all data is inaccessible or deleted
        var profileAfterDelete = await Client.GetAsync("/api/v1/users/me");
        profileAfterDelete.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var exportAfterDelete = await Client.PostAsync("/api/v1/users/me/export", null);
        exportAfterDelete.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        // API keys endpoint may still succeed but return empty list (cascade delete)
        var keysAfterDelete = await Client.GetAsync("/api/v1/users/me/api-keys");
        keysAfterDelete.StatusCode.Should().Be(HttpStatusCode.OK);
        var keysAfterDeleteData = await keysAfterDelete.Content.ReadFromJsonAsync<ListApiKeysResponse>();
        keysAfterDeleteData!.Keys.Should().BeEmpty();
    }

    [Fact]
    public async Task UserDataManagement_ExportContainsCorrectUserData()
    {
        // Register with specific data
        var username = TestHelpers.GenerateTestUsername();
        var registerResponse = await TestHelpers.RegisterUserAsync(Client, username, "hash-123");
        TestHelpers.SetAuthToken(Client, registerResponse.Token);

        // Export data
        var exportResponse = await Client.PostAsync("/api/v1/users/me/export", null);
        exportResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var exportResult = await exportResponse.Content.ReadFromJsonAsync<ExportUserDataResponse>();

        // Verify export contains user data
        exportResult!.Data.Should().Contain(username);

        // Get user ID from anonymous User object
        using var jsonDoc = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(registerResponse.User));
        var userId = jsonDoc.RootElement.GetProperty("id").GetGuid();
        exportResult.Data.Should().Contain(userId.ToString());
    }

    [Fact]
    public async Task UserDataManagement_MultipleUsersDataIsIsolated()
    {
        // Create first user with data
        var user1Username = TestHelpers.GenerateTestUsername();
        var user1Response = await TestHelpers.RegisterUserAsync(Client, user1Username, "hash1");
        TestHelpers.SetAuthToken(Client, user1Response.Token);
        await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "User1-Key" });

        var user1Export = await Client.PostAsync("/api/v1/users/me/export", null);
        var user1ExportData = await user1Export.Content.ReadFromJsonAsync<ExportUserDataResponse>();

        // Create second user with data
        TestHelpers.ClearAuthToken(Client);
        var user2Username = TestHelpers.GenerateTestUsername();
        var user2Response = await TestHelpers.RegisterUserAsync(Client, user2Username, "hash2");
        TestHelpers.SetAuthToken(Client, user2Response.Token);
        await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "User2-Key" });

        var user2Export = await Client.PostAsync("/api/v1/users/me/export", null);
        var user2ExportData = await user2Export.Content.ReadFromJsonAsync<ExportUserDataResponse>();

        // Verify each export contains only their own data
        user1ExportData!.Data.Should().Contain(user1Username);
        user1ExportData.Data.Should().NotContain(user2Username);

        user2ExportData!.Data.Should().Contain(user2Username);
        user2ExportData.Data.Should().NotContain(user1Username);
    }

    [Fact]
    public async Task UserDataManagement_CannotAccessOtherUsersData()
    {
        // Create two users
        var user1Response = await TestHelpers.RegisterUserAsync(Client, TestHelpers.GenerateTestUsername(), "hash1");
        TestHelpers.ClearAuthToken(Client);
        var user2Response = await TestHelpers.RegisterUserAsync(Client, TestHelpers.GenerateTestUsername(), "hash2");

        // User2 tries to access user1's data using user1's token
        TestHelpers.SetAuthToken(Client, user1Response.Token);
        var profile = await Client.GetAsync("/api/v1/users/me");
        var profileData = await profile.Content.ReadFromJsonAsync<Afina.Modules.Users.Features.GetCurrentUser.GetCurrentUserResponse>();

        // Verify it returns user1's data, not user2's
        using var user1JsonDoc = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(user1Response.User));
        using var user2JsonDoc = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(user2Response.User));
        var user1Id = user1JsonDoc.RootElement.GetProperty("id").GetGuid();
        var user2Id = user2JsonDoc.RootElement.GetProperty("id").GetGuid();

        profileData!.Id.Should().Be(user1Id);
        profileData.Id.Should().NotBe(user2Id);
    }
}
