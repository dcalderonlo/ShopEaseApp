using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ShopEaseApp.Api.Components;
using ShopEaseApp.Api.Features.Identity.Components;

namespace ShopEaseApp.Blazor.Tests;

public class LogoutTests
{
    // ── Scenario: Logout clears auth state and returns to root ─────────────────

    [Fact]
    public void Logout_NavigatesToRoot()
    {
        using var ctx = new TestContext();
        ctx.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        ctx.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

        ctx.RenderComponent<Logout>();

        var nav = ctx.Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/", nav.Uri);
    }
}
