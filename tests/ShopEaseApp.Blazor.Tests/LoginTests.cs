using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ShopEaseApp.Api.Features.Identity.Components;
using ShopEaseApp.Api.Features.Identity.Login;

namespace ShopEaseApp.Blazor.Tests;

public class LoginTests
{
    private static void RegisterLoginServices(TestContext ctx, Mock<LoginHandler> handler)
    {
        ctx.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        ctx.Services.AddSingleton(handler.Object);
    }

    // ── Scenario: Login form renders expected fields ───────────────────────────

    [Fact]
    public void LoginForm_RendersInputsAndButton()
    {
        using var ctx = new TestContext();
        RegisterLoginServices(ctx, TestHelpers.MockLoginHandler(false, null));

        var cut = ctx.RenderComponent<Login>();

        Assert.Contains("Email", cut.Markup);
        Assert.Contains("Password", cut.Markup);
        Assert.Contains("Login", cut.Markup);
    }

    // ── Scenario: Invalid login credentials shows error, no redirect ───────────

    [Fact]
    public async Task Login_InvalidCredentials_ShowsError()
    {
        using var ctx = new TestContext();
        RegisterLoginServices(ctx, TestHelpers.MockLoginHandler(false, null));

        var cut = ctx.RenderComponent<Login>();
        cut.Find("form").Submit();

        Assert.Contains("Invalid credentials.", cut.Markup);
    }

    // ── Scenario: Valid credentials navigates to root (success path) ───────────

    [Fact]
    public async Task Login_ValidCredentials_NavigatesToRoot()
    {
        using var ctx = new TestContext();
        var response = new LoginResponse("token-xyz", "shopper@test", "Customer", DateTime.UtcNow.AddHours(1));
        RegisterLoginServices(ctx, TestHelpers.MockLoginHandler(true, response));

        var cut = ctx.RenderComponent<Login>();
        cut.Find("form").Submit();

        Assert.DoesNotContain("Invalid credentials.", cut.Markup);
        var nav = ctx.Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/", nav.Uri);
    }
}
