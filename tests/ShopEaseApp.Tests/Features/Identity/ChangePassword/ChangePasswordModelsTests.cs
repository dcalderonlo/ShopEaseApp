using ShopEaseApp.Api.Features.Identity.ChangePassword;

namespace ShopEaseApp.Tests.Features.Identity.ChangePassword;

/// <summary>
/// Tests for the ChangePasswordRequest record contract.
/// </summary>
public class ChangePasswordModelsTests
{
    // ── Scenario: Request constructs with the right values ────────────────────

    [Fact]
    public void ChangePasswordRequest_Constructs_WithExpectedValues()
    {
        var request = new ChangePasswordRequest("current-pass", "new-pass");

        Assert.Equal("current-pass", request.CurrentPassword);
        Assert.Equal("new-pass", request.NewPassword);
    }

    // ── Triangulation: record deconstruction ──────────────────────────────────

    [Fact]
    public void ChangePasswordRequest_Deconstructs_ToPositionalValues()
    {
        var request = new ChangePasswordRequest("old", "new");

        var (current, next) = request;

        Assert.Equal("old", current);
        Assert.Equal("new", next);
    }

    // ── Triangulation: record value equality ──────────────────────────────────

    [Fact]
    public void ChangePasswordRequest_WithSameValues_AreEqual()
    {
        var a = new ChangePasswordRequest("abc", "xyz");
        var b = new ChangePasswordRequest("abc", "xyz");

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
