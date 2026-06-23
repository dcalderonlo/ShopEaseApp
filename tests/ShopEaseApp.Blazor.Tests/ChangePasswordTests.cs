using Bunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ShopEaseApp.Api.Features.Identity.ChangePassword;
using ShopEaseApp.Api.Infrastructure.Data;
using ChangePasswordPage = ShopEaseApp.Api.Components.DesignSystem.ChangePasswordPage;

namespace ShopEaseApp.Blazor.Tests;

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
        um.Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);
        return um;
    }

    private static void RegisterServices(TestContext ctx, Mock<ChangePasswordHandler> handler, AppUser user)
    {
        ctx.Services.AddSingleton(handler.Object);
        ctx.Services.AddSingleton(MockUserManager(user).Object);
    }

    private static void FillForm(IRenderedComponent<ChangePasswordPage> cut,
        string current = "oldPass", string newPass = "newPass123", string confirm = "newPass123")
    {
        // Fill each input separately to avoid stale element references after re-renders
        cut.Find("input[id='current-password']").Input(current);
        cut.Find("input[id='new-password']").Input(newPass);
        cut.Find("input[id='confirm-password']").Input(confirm);
    }

    // ── Scenario: form renders inputs and button ──────────────────────────────

    [Fact]
    public void ChangePasswordForm_RendersInputsAndButton()
    {
        using var ctx = new TestContext();
        var user = new AppUser();
        RegisterServices(ctx, MockHandler(true, null), user);

        var cut = ctx.RenderWithAuth<ChangePasswordPage>(TestHelpers.Authenticated("u1"));

        Assert.Contains("Current Password", cut.Markup);
        Assert.Contains("New Password", cut.Markup);
        Assert.Contains("Confirm New Password", cut.Markup);
        Assert.Contains("Update Password", cut.Markup);
    }

    // ── Scenario: valid submit calls handler and shows success ─────────────────

    [Fact]
    public void ChangePassword_ValidSubmit_ShowsSuccessAndCallsHandler()
    {
        using var ctx = new TestContext();
        var user = new AppUser();
        var handlerMock = MockHandler(true, null);
        RegisterServices(ctx, handlerMock, user);

        var cut = ctx.RenderWithAuth<ChangePasswordPage>(TestHelpers.Authenticated("u1"));
        FillForm(cut);

        cut.Find("form").Submit();

        handlerMock.Verify(
            h => h.HandleAsync(It.IsAny<AppUser>(), It.Is<ChangePasswordRequest>(
                r => r.CurrentPassword == "oldPass" && r.NewPassword == "newPass123")),
            Times.Once);
    }

    // ── Scenario: handler error is displayed ──────────────────────────────────

    [Fact]
    public void ChangePassword_Failure_ShowsError()
    {
        using var ctx = new TestContext();
        var user = new AppUser();
        var handlerMock = MockHandler(false, "Incorrect password.");
        RegisterServices(ctx, handlerMock, user);

        var cut = ctx.RenderWithAuth<ChangePasswordPage>(TestHelpers.Authenticated("u1"));
        FillForm(cut);

        cut.Find("form").Submit();

        Assert.Contains("Incorrect password.", cut.Markup);
    }

    // ── Scenario: passwords don't match ───────────────────────────────────────

    [Fact]
    public void ChangePassword_MismatchedConfirm_ShowsError()
    {
        using var ctx = new TestContext();
        var user = new AppUser();
        var handlerMock = MockHandler(true, null);
        RegisterServices(ctx, handlerMock, user);

        var cut = ctx.RenderWithAuth<ChangePasswordPage>(TestHelpers.Authenticated("u1"));
        FillForm(cut, confirm: "different");

        cut.Find("form").Submit();

        Assert.Contains("Passwords do not match", cut.Markup);
        handlerMock.Verify(
            h => h.HandleAsync(It.IsAny<AppUser>(), It.IsAny<ChangePasswordRequest>()),
            Times.Never);
    }
}
