using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Afina.Modules.Users.Features.CreateApiKey;
using Afina.Modules.Users.Features.GetCurrentUser;
using Afina.Modules.Users.Features.ListApiKeys;
using Afina.Modules.Users.Features.RefreshToken;
using FluentAssertions;
using Xunit;

namespace Afina.Modules.Users.Tests.Scenarios;

/// <summary>
/// Complete user lifecycle scenario from registration to deletion
/// </summary>
public class CompleteUserLifecycleTests : UsersIntegrationTestBase
{
    public CompleteUserLifecycleTests(DatabaseFixture dbFixture) : base(dbFixture) { }

    [Fact]
    public async Task CompleteUserLifecycle_RegisterLoginCreateKeysExportDelete_WorksEndToEnd()
    {
        // 1. Register
        var username = TestHelpers.GenerateTestUsername();
        var passwordHash = "secure-hash-123";
        var registerResponse = await TestHelpers.RegisterUserAsync(Client, username, passwordHash, "hint");

        registerResponse.UserId.Should().NotBeEmpty();
        var userId = registerResponse.UserId;

        // 2. Login with credentials
        TestHelpers.ClearAuthToken(Client);
        var loginResponse = await TestHelpers.LoginUserAsync(Client, username, passwordHash);

        // Set new login token
        TestHelpers.SetAuthToken(Client, loginResponse.Token);        // 3. Get current user profile
        var profileResponse = await Client.GetAsync("/api/v1/users/me");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await profileResponse.Content.ReadFromJsonAsync<GetCurrentUserResponse>();
        profile!.Id.Should().Be(userId);
        profile.Username.Should().Be(username);

        // 4. Create multiple API keys
        await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "Production Key" });
        await Client.PostAsJsonAsync("/api/v1/users/me/api-keys", new CreateApiKeyRequest { Name = "Development Key" });

        var keysResponse = await Client.GetAsync("/api/v1/users/me/api-keys");
        var keys = await keysResponse.Content.ReadFromJsonAsync<ListApiKeysResponse>();
        keys!.Keys.Should().HaveCount(2);

        // 5. Export user data
        var exportResponse = await Client.PostAsync("/api/v1/users/me/export", null);
        exportResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 6. Refresh token
        var refreshRequest = new RefreshTokenRequest { RefreshToken = loginResponse.RefreshToken };
        var refreshResponse = await Client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 7. Delete user
        var deleteResponse = await Client.DeleteAsync("/api/v1/users/me");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 8. Verify user is completely deleted
        var profileAfterDelete = await Client.GetAsync("/api/v1/users/me");
        profileAfterDelete.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // 9. Verify cannot login anymore
        TestHelpers.ClearAuthToken(Client);
        var loginAfterDelete = await Client.PostAsJsonAsync("/api/v1/auth/login",
            new Afina.Modules.Users.Features.Login.LoginRequest { Username = username, AuthHash = passwordHash });
        loginAfterDelete.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
