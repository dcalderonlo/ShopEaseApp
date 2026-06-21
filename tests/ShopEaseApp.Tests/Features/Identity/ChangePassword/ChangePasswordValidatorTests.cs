using FluentValidation.TestHelper;
using ShopEaseApp.Api.Features.Identity.ChangePassword;

namespace ShopEaseApp.Tests.Features.Identity.ChangePassword;

/// <summary>
/// Tests for ChangePasswordValidator rules (FluentValidation).
/// </summary>
public class ChangePasswordValidatorTests
{
    private readonly ChangePasswordValidator _validator = new();

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ValidRequest_PassesAllRules()
    {
        var result = _validator.TestValidate(new ChangePasswordRequest("current-pass", "new-password"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── Edge / error cases (triangulation) ───────────────────────────────────

    [Theory]
    [InlineData("", "new-password", "CurrentPassword")]
    [InlineData("current-pass", "", "NewPassword")]
    [InlineData("current-pass", "12345", "NewPassword")] // 5 chars < 6 minimum
    public void Validate_InvalidField_FailsWithCorrectProperty(
        string current, string next, string expectedProperty)
    {
        var result = _validator.TestValidate(new ChangePasswordRequest(current, next));

        result.ShouldHaveValidationErrorFor(expectedProperty);
    }
}
