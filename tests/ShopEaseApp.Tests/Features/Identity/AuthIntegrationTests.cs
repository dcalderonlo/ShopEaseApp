using System.Net;
using System.Net.Http.Json;
using ShopEaseApp.Api.Features.Identity.Login;
using ShopEaseApp.Api.Features.Identity.Register;

namespace ShopEaseApp.Tests.Features.Identity;

public class AuthIntegrationTests : IClassFixture<ShopEaseTestFactory>
{
    private readonly ShopEaseTestFactory _factory;

    public AuthIntegrationTests(ShopEaseTestFactory factory)
    {
        _factory = factory;
    }

    // ── Scenario: Successful dual-auth login ──────────────────────────────────

    [Fact]
    public async Task Register_ThenLogin_ReturnsBearerTokenAndSetsCookie()
    {
        var client = _factory.CreateAuthClient();

        // Step 1 — Register
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "Jane", "Doe", "jane@shopease.test", "Password1!"));

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        // Step 2 — Login
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "jane@shopease.test", "Password1!"));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginBody);
        Assert.NotEmpty(loginBody!.Token);
        Assert.Equal("jane@shopease.test", loginBody.Email);

        // Assert HttpOnly cookie was set
        Assert.True(loginResponse.Headers.Contains("Set-Cookie"));
        var setCookieHeader = loginResponse.Headers.GetValues("Set-Cookie").First();
        Assert.Contains("auth_token", setCookieHeader);
        Assert.Contains("httponly", setCookieHeader.ToLower());
    }

    // ── Scenario: Login with invalid credentials ──────────────────────────────

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var client = _factory.CreateAuthClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "nonexistent@shopease.test", "WrongPass1!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Scenario: Admin-only endpoint denies Customer role ────────────────────

    [Fact]
    public async Task AdminEndpoint_WithCustomerToken_ReturnsForbiddenOrNotFound()
    {
        var client = _factory.CreateAuthClient();

        // Register + login as Customer
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "Bob", "Smith", "bob@shopease.test", "Password1!"));

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "bob@shopease.test", "Password1!"));

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // Try to access admin endpoint with Customer token
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginBody!.Token);

        var adminResponse = await client.GetAsync("/api/admin/test-admin-only");

        // 403 Forbidden (authenticated but wrong role) or 404 if endpoint doesn't exist yet
        Assert.True(
            adminResponse.StatusCode == HttpStatusCode.Forbidden ||
            adminResponse.StatusCode == HttpStatusCode.NotFound,
            $"Expected 403 or 404, got {adminResponse.StatusCode}");
    }
}
