using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Tests.Features.Identity.ChangePassword;

/// <summary>
/// Tests for the new MustChangePassword flag on AppUser (forced first-change flow).
/// </summary>
public class AppUserTests
{
    // ── Scenario: Flag defaults to false (existing users unaffected) ──────────

    [Fact]
    public void AppUser_MustChangePassword_DefaultsToFalse()
    {
        var user = new AppUser();

        Assert.False(user.MustChangePassword);
    }

    // ── Triangulation: flag is settable ───────────────────────────────────────

    [Fact]
    public void AppUser_MustChangePassword_CanBeSetToTrue()
    {
        var user = new AppUser { MustChangePassword = true };

        Assert.True(user.MustChangePassword);
    }
}
