using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using ShopEaseApp.Api.Components;

namespace ShopEaseApp.Tests.Components;

public class ServerAuthenticationStateProviderTests
{
    private static ClaimsPrincipal AuthenticatedUser(string email)
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-123"),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, "Customer")
            },
            authenticationType: "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    // ── Scenario: Authenticated HttpContext propagates into Blazor circuit ───

    [Fact]
    public async Task GetAuthenticationStateAsync_ReturnsAuthenticatedUserFromHttpContext()
    {
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = AuthenticatedUser("shopper@test") }
        };
        var provider = new ServerAuthenticationStateProvider(accessor);

        var state = await provider.GetAuthenticationStateAsync();

        Assert.NotNull(state);
        Assert.True(state.User.Identity?.IsAuthenticated);
        Assert.Equal("shopper@test", state.User.FindFirstValue(ClaimTypes.Email));
        Assert.Equal("user-123", state.User.FindFirstValue(ClaimTypes.NameIdentifier));
    }

    // ── Scenario: No HttpContext (SignalR post-back / prerender gap) → anonymous

    [Fact]
    public async Task GetAuthenticationStateAsync_ReturnsAnonymousWhenHttpContextIsNull()
    {
        var accessor = new HttpContextAccessor { HttpContext = null };
        var provider = new ServerAuthenticationStateProvider(accessor);

        var state = await provider.GetAuthenticationStateAsync();

        Assert.NotNull(state);
        Assert.False(state.User.Identity?.IsAuthenticated);
    }

    // ── Scenario: NotifyStateChanged refreshes the cached state ────────────────

    [Fact]
    public async Task NotifyStateChanged_RaisesAuthenticationStateChangedEvent()
    {
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = AuthenticatedUser("changed@test") }
        };
        var provider = new ServerAuthenticationStateProvider(accessor);

        AuthenticationState? raised = null;
        provider.AuthenticationStateChanged += async t => raised = await t;
        provider.NotifyStateChanged();

        Assert.NotNull(raised);
        Assert.Equal("changed@test", raised!.User.FindFirstValue(ClaimTypes.Email));
    }
}
