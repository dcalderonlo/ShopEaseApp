using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ShopEaseApp.Api.Features.Identity.Components;
using ShopEaseApp.Api.Features.Identity.Register;

namespace ShopEaseApp.Blazor.Tests;

public class RegisterTests
{
    // ── Scenario: Register form renders expected fields ────────────────────────

    [Fact]
    public void RegisterForm_RendersInputsAndButton()
    {
        using var ctx = new TestContext();
        var handler = TestHelpers.MockRegisterHandler(true, null, []);
        ctx.Services.AddSingleton(handler.Object);

        var cut = ctx.RenderComponent<Register>();

        Assert.Contains("First Name", cut.Markup);
        Assert.Contains("Last Name", cut.Markup);
        Assert.Contains("Email", cut.Markup);
        Assert.Contains("Password", cut.Markup);
        Assert.Contains("Register", cut.Markup);
    }

    // ── Scenario: Guest registers → redirected to login ────────────────────────

    [Fact]
    public async Task Register_Success_RedirectsToLogin()
    {
        using var ctx = new TestContext();
        var handler = TestHelpers.MockRegisterHandler(
            true, new RegisterResponse("u1", "new@test", "Customer"), []);
        ctx.Services.AddSingleton(handler.Object);

        var cut = ctx.RenderComponent<Register>();
        cut.Find("form").Submit();

        var nav = ctx.Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/login", nav.Uri);
    }

    // ── Scenario: Registration failure shows errors ────────────────────────────

    [Fact]
    public async Task Register_Failure_ShowsErrors()
    {
        using var ctx = new TestContext();
        var handler = TestHelpers.MockRegisterHandler(false, null, new[] { "Email already taken" });
        ctx.Services.AddSingleton(handler.Object);

        var cut = ctx.RenderComponent<Register>();
        cut.Find("form").Submit();

        Assert.Contains("Email already taken", cut.Markup);
    }
}
