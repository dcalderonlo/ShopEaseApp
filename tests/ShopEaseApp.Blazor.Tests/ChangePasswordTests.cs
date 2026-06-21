using System.Security.Claims;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ShopEaseApp.Api.Features.Identity.ChangePassword;
using ShopEaseApp.Api.Infrastructure.Data;
// The component class name "ChangePassword" collides with the
// ShopEaseApp.Api.Features.Identity.ChangePassword namespace, so alias the component.
using ChangePasswordPage = ShopEaseApp.Api.Features.Identity.Components.ChangePassword;

namespace ShopEaseApp.Blazor.Tests;

/// <summary>
/// bUnit tests for the ChangePassword Blazor page. Mocks only the handler and
/// UserManager (2 mocks) — healthy mock ratio. Auth state is provided via
/// RenderWithAuth (FakeAuthStateProvider).
/// </summary>
public class ChangePasswordTests
{
    private static Mock<ChangePasswordHandler> MockHandler(bool success, string? error)
    {
        var mock = new Mock<ChangePasswordHandler>(null!) { CallBase = false };
        mock.Setup(h => h.HandleAsync(It.IsAny<AppUser>(), It.IsAny<ChangePasswordRequest>()))
            .ReturnsAsync((success, error));
        return mock;
    }

    private static Mock<UserManager<AppUser>> MockUserManager(AppUser? user)
    {
        var storeMock = new Mock<IUserStore<AppUser>>();
        var um = new Mock<UserManager<AppUser>>(
            storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        um.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        return um;
    }

    private static void RegisterServices(TestContext ctx, Mock<ChangePasswordHandler> handler, AppUser user)
    {
        ctx.Services.AddSingleton(handler.Object);
        ctx.Services.AddSingleton(MockUserManager(user).Object);
    }

    // ── Scenario: form renders the expected inputs and button ─────────────────

    [Fact]
    public void ChangePasswordForm_RendersInputsAndButton()
    {
        using var ctx = new TestContext();
        var user = new AppUser();
        RegisterServices(ctx, MockHandler(true, null), user);

        var cut = ctx.RenderWithAuth<ChangePasswordPage>(TestHelpers.Authenticated("u1"));

        Assert.Contains("Current Password", cut.Markup);
        Assert.Contains("New Password", cut.Markup);
        Assert.Contains("Change Password", cut.Markup);
    }

    // ── Scenario: valid submit shows success and calls the handler ────────────

    [Fact]
    public async Task ChangePassword_ValidSubmit_ShowsSuccessAndCallsHandler()
    {
        using var ctx = new TestContext();
        var user = new AppUser();
        var handlerMock = MockHandler(true, null);
        RegisterServices(ctx, handlerMock, user);

        var cut = ctx.RenderWithAuth<ChangePasswordPage>(TestHelpers.Authenticated("u1"));
        cut.Find("form").Submit();

        Assert.Contains("Password changed", cut.Markup);
        handlerMock.Verify(
            h => h.HandleAsync(It.IsAny<AppUser>(), It.IsAny<ChangePasswordRequest>()), Times.Once);
    }

    // ── Scenario: failed change shows the error message ───────────────────────

    [Fact]
    public async Task ChangePassword_Failure_ShowsError()
    {
        using var ctx = new TestContext();
        var user = new AppUser();
        var handlerMock = MockHandler(false, "Incorrect password.");
        RegisterServices(ctx, handlerMock, user);

        var cut = ctx.RenderWithAuth<ChangePasswordPage>(TestHelpers.Authenticated("u1"));
        cut.Find("form").Submit();

        Assert.Contains("Incorrect password.", cut.Markup);
    }
}
