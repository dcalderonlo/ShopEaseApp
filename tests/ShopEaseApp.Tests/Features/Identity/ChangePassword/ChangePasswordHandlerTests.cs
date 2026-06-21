using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ShopEaseApp.Api.Features.Identity.ChangePassword;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Tests.Features.Identity.ChangePassword;

/// <summary>
/// Unit tests for ChangePasswordHandler — the core behavior of the password
/// change flow. Mocks only UserManager (single dependency) — healthy mock ratio.
/// </summary>
public class ChangePasswordHandlerTests
{
    private static Mock<UserManager<AppUser>> BuildUserManagerMock()
    {
        var storeMock = new Mock<IUserStore<AppUser>>();
        return new Mock<UserManager<AppUser>>(
            storeMock.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<AppUser>>().Object,
            Array.Empty<IUserValidator<AppUser>>(),
            Array.Empty<IPasswordValidator<AppUser>>(),
            new UpperInvariantLookupNormalizer(),
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<AppUser>>>().Object);
    }

    // ── Scenario: Successful password change clears the forced-change flag ────

    [Fact]
    public async Task HandleAsync_ValidChange_ReturnsSuccessAndClearsFlag()
    {
        var um = BuildUserManagerMock();
        um.Setup(m => m.ChangePasswordAsync(It.IsAny<AppUser>(), "old-pass", "new-pass"))
            .ReturnsAsync(IdentityResult.Success);
        um.Setup(m => m.UpdateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var handler = new ChangePasswordHandler(um.Object);
        var user = new AppUser { MustChangePassword = true };

        var (success, error) = await handler.HandleAsync(user, new ChangePasswordRequest("old-pass", "new-pass"));

        Assert.True(success);
        Assert.Null(error);
        Assert.False(user.MustChangePassword); // flag cleared atomically
        um.Verify(m => m.UpdateAsync(It.Is<AppUser>(u => !u.MustChangePassword)), Times.Once);
    }

    // ── Scenario: Incorrect current password is rejected, flag preserved ──────

    [Fact]
    public async Task HandleAsync_WrongCurrentPassword_ReturnsFailureWithError()
    {
        var um = BuildUserManagerMock();
        um.Setup(m => m.ChangePasswordAsync(It.IsAny<AppUser>(), "wrong-pass", "new-pass"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Incorrect password." }));

        var handler = new ChangePasswordHandler(um.Object);
        var user = new AppUser { MustChangePassword = true };

        var (success, error) = await handler.HandleAsync(user, new ChangePasswordRequest("wrong-pass", "new-pass"));

        Assert.False(success);
        Assert.Equal("Incorrect password.", error);
        Assert.True(user.MustChangePassword); // flag NOT cleared on failure
        um.Verify(m => m.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
    }

    // ── Scenario: New password does not meet requirements (Identity rejects) ──

    [Fact]
    public async Task HandleAsync_ShortNewPassword_ReturnsFailureWithError()
    {
        var um = BuildUserManagerMock();
        um.Setup(m => m.ChangePasswordAsync(It.IsAny<AppUser>(), "old-pass", "12345"))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Passwords must be at least 6 characters." }));

        var handler = new ChangePasswordHandler(um.Object);
        var user = new AppUser();

        var (success, error) = await handler.HandleAsync(user, new ChangePasswordRequest("old-pass", "12345"));

        Assert.False(success);
        Assert.Contains("6 characters", error);
    }

    // ── Triangulation: success with flag already false skips the extra Update ─

    [Fact]
    public async Task HandleAsync_ValidChange_FlagAlreadyFalse_DoesNotCallUpdate()
    {
        var um = BuildUserManagerMock();
        um.Setup(m => m.ChangePasswordAsync(It.IsAny<AppUser>(), "old-pass", "new-pass"))
            .ReturnsAsync(IdentityResult.Success);

        var handler = new ChangePasswordHandler(um.Object);
        var user = new AppUser { MustChangePassword = false };

        var (success, error) = await handler.HandleAsync(user, new ChangePasswordRequest("old-pass", "new-pass"));

        Assert.True(success);
        Assert.Null(error);
        um.Verify(m => m.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
    }
}
