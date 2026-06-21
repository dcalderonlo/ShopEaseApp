using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ShopEaseApp.Api.Components.Layout;
using ShopEaseApp.Api.Features.Cart;

namespace ShopEaseApp.Blazor.Tests;

public class NavMenuTests
{
    // ── Scenario: Navbar reflects guest state ──────────────────────────────────

    [Fact]
    public async Task Navbar_Guest_ShowsLoginAndRegister_NoLogout()
    {
        using var ctx = new TestContext();
        await using var db = TestHelpers.CreateDb();
        var (cacheMock, _) = TestHelpers.CreateCache();

        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped(_ => cacheMock.Object);
        ctx.Services.AddScoped<CartService>();

        var cut = ctx.RenderWithAuth<NavMenu>(TestHelpers.Anonymous());

        Assert.Contains("Login", cut.Markup);
        Assert.Contains("Register", cut.Markup);
        Assert.DoesNotContain("Logout", cut.Markup);
    }

    // ── Scenario: Navbar reflects authenticated state ──────────────────────────

    [Fact]
    public async Task Navbar_Authenticated_ShowsUsernameAndLogout_NoLogin()
    {
        using var ctx = new TestContext();
        await using var db = TestHelpers.CreateDb();
        var (cacheMock, _) = TestHelpers.CreateCache();

        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped(_ => cacheMock.Object);
        ctx.Services.AddScoped<CartService>();

        var cut = ctx.RenderWithAuth<NavMenu>(TestHelpers.Authenticated("u1", "alice@test", "Customer"));

        Assert.Contains("alice@test", cut.Markup);
        Assert.Contains("Logout", cut.Markup);
        Assert.DoesNotContain(">Login<", cut.Markup);
        Assert.DoesNotContain("Register", cut.Markup);
    }
}
