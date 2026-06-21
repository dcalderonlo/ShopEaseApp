using System.Net;
using System.Net.Http.Json;
using ShopEaseApp.Api.Features.Identity.ChangePassword;
using ShopEaseApp.Api.Features.Identity.Login;
using ShopEaseApp.Api.Features.Identity.Register;

namespace ShopEaseApp.Tests.Features.Identity;

/// <summary>
/// Integration tests for POST /api/auth/change-password via WebApplicationFactory.
/// Exercises the real Identity store + endpoint + validator pipeline.
/// </summary>
public class ChangePasswordIntegrationTests(ShopEaseTestFactory factory) : IClassFixture<ShopEaseTestFactory>
{
    private readonly ShopEaseTestFactory _factory = factory;

    /// <summary>Register + login, returning an authenticated client and the current password.</summary>
    private async Task<(HttpClient client, string email, string currentPassword)> CreateAuthenticatedClientAsync(string email)
    {
        var client = _factory.CreateAuthClient();
        const string password = "Password1!";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Change", "User", email, password));

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginBody!.Token);

        return (client, email, password);
    }

    // ── Scenario: Successful password change → user can login with new password ─

    [Fact]
    public async Task ChangePassword_WithValidTokenAndCredentials_ReturnsOkAndAllowsNewLogin()
    {
        var (client, email, currentPassword) = await CreateAuthenticatedClientAsync("changer1@shopease.test");
        const string newPassword = "NewPassword1!";

        var response = await client.PostAsJsonAsync("/api/auth/change-password",
            new ChangePasswordRequest(currentPassword, newPassword));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Spec: "the user can login with the new password"
        var newLogin = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, newPassword));
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }

    // ── Scenario: Incorrect current password is rejected ───────────────────────

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_ReturnsBadRequest()
    {
        var (client, _, _) = await CreateAuthenticatedClientAsync("changer2@shopease.test");

        var response = await client.PostAsJsonAsync("/api/auth/change-password",
            new ChangePasswordRequest("WrongCurrentPass!", "NewPassword1!"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Scenario: Unauthorized when no token is supplied ───────────────────────

    [Fact]
    public async Task ChangePassword_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateAuthClient();

        var response = await client.PostAsJsonAsync("/api/auth/change-password",
            new ChangePasswordRequest("any-current", "NewPassword1!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Scenario: New password too short — validator rejects ───────────────────

    [Fact]
    public async Task ChangePassword_WithShortNewPassword_ReturnsBadRequest()
    {
        var (client, _, currentPassword) = await CreateAuthenticatedClientAsync("changer3@shopease.test");

        var response = await client.PostAsJsonAsync("/api/auth/change-password",
            new ChangePasswordRequest(currentPassword, "12345"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
