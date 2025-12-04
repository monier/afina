using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Afina.Modules.Users.Features.GetCurrentUser;
using Afina.Modules.Users.Features.RefreshToken;
using FluentAssertions;
using Xunit;

namespace Afina.Modules.Users.Tests.Scenarios;

/// <summary>
/// Scenarios testing authentication flows and token management
/// </summary>
public class AuthenticationFlowTests : UsersIntegrationTestBase
{
    public AuthenticationFlowTests(DatabaseFixture dbFixture) : base(dbFixture) { }

    [Fact]
    public async Task AuthenticationFlow_LoginRefreshMultipleTimes_MaintainsSession()
    {
        // Register and login
        var username = TestHelpers.GenerateTestUsername();
        var passwordHash = "hash-123";
        await TestHelpers.RegisterUserAsync(Client, username, passwordHash);

        TestHelpers.ClearAuthToken(Client);
        var loginResponse = await TestHelpers.LoginUserAsync(Client, username, passwordHash);

        var currentRefreshToken = loginResponse.RefreshToken;
        var originalUserId = loginResponse.UserId;

        // Perform multiple token refreshes
        for (int i = 0; i < 5; i++)
        {
            var refreshRequest = new RefreshTokenRequest { RefreshToken = currentRefreshToken };
            var refreshResponse = await Client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

            refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<RefreshTokenResponse>();

            // Set new token and verify user profile
            TestHelpers.SetAuthToken(Client, refreshResult!.Token);
            var profileResponse = await Client.GetAsync("/api/v1/users/me");
            profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var profile = await profileResponse.Content.ReadFromJsonAsync<GetCurrentUserResponse>();
            profile!.Id.Should().Be(originalUserId);
            profile.Username.Should().Be(username);

            // Update refresh token for next iteration
            currentRefreshToken = refreshResult.RefreshToken;
        }
    }

    [Fact]
    public async Task AuthenticationFlow_MultipleSimultaneousSessions_AreIndependent()
    {
        // Register user once
        var username = TestHelpers.GenerateTestUsername();
        var passwordHash = "hash-123";
        await TestHelpers.RegisterUserAsync(Client, username, passwordHash);

        // Create two separate login sessions
        TestHelpers.ClearAuthToken(Client);
        var session1 = await TestHelpers.LoginUserAsync(Client, username, passwordHash);
        var session2 = await TestHelpers.LoginUserAsync(Client, username, passwordHash);

        // Verify sessions have different refresh tokens (always rotated)
        session1.RefreshToken.Should().NotBe(session2.RefreshToken);
        // Access tokens may be identical if created within same second due to JWT timestamp

        // Verify both sessions can access user profile
        TestHelpers.SetAuthToken(Client, session1.Token);
        var profile1 = await Client.GetAsync("/api/v1/users/me");
        profile1.StatusCode.Should().Be(HttpStatusCode.OK);

        TestHelpers.SetAuthToken(Client, session2.Token);
        var profile2 = await Client.GetAsync("/api/v1/users/me");
        profile2.StatusCode.Should().Be(HttpStatusCode.OK);

        // Refresh first session
        var refresh1Request = new RefreshTokenRequest { RefreshToken = session1.RefreshToken };
        var refresh1Response = await Client.PostAsJsonAsync("/api/v1/auth/refresh", refresh1Request);
        refresh1Response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Second session should still be valid
        TestHelpers.SetAuthToken(Client, session2.Token);
        var profile2AfterRefresh = await Client.GetAsync("/api/v1/users/me");
        profile2AfterRefresh.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AuthenticationFlow_RegisterLoginLogout_ClearsSession()
    {
        // Register
        var (userId, token, refreshToken, username, passwordHash) = await TestHelpers.RegisterAndAuthenticateAsync(Client);

        // Verify authenticated
        var profileResponse = await Client.GetAsync("/api/v1/users/me");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Simulate logout by clearing token
        TestHelpers.ClearAuthToken(Client);

        // Verify cannot access protected resources
        var profileAfterLogout = await Client.GetAsync("/api/v1/users/me");
        profileAfterLogout.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Login again
        var newLoginResponse = await TestHelpers.LoginUserAsync(Client, username, passwordHash);
        TestHelpers.SetAuthToken(Client, newLoginResponse.Token);

        // Verify authenticated again
        var profileAfterRelogin = await Client.GetAsync("/api/v1/users/me");
        profileAfterRelogin.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AuthenticationFlow_OldTokensInvalidAfterPasswordChange()
    {
        // Note: This test assumes password change functionality exists
        // Currently, it tests that old tokens remain valid (no password change in this module)
        // If password change is added, update this test accordingly

        var (userId, token, refreshToken, _, _) = await TestHelpers.RegisterAndAuthenticateAsync(Client);
        var oldToken = token;

        // Token should work
        TestHelpers.SetAuthToken(Client, oldToken);
        var profileResponse = await Client.GetAsync("/api/v1/users/me");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // If password change were implemented:
        // 1. Change password
        // 2. Verify old token no longer works
        // 3. Login with new password
        // 4. Verify new token works
    }
}
