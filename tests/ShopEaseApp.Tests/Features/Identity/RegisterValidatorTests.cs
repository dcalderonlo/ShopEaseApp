using FluentValidation.TestHelper;
using ShopEaseApp.Api.Features.Identity.Register;

namespace ShopEaseApp.Tests.Features.Identity;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ValidRequest_PassesAllRules()
    {
        var request = new RegisterRequest("Jane", "Doe", "jane@example.com", "Password1!");
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── Edge / error cases (triangulation) ───────────────────────────────────

    [Theory]
    [InlineData("", "Doe", "jane@example.com", "Password1!", "FirstName")]
    [InlineData("Jane", "", "jane@example.com", "Password1!", "LastName")]
    [InlineData("Jane", "Doe", "not-an-email", "Password1!", "Email")]
    [InlineData("Jane", "Doe", "", "Password1!", "Email")]
    [InlineData("Jane", "Doe", "jane@example.com", "short", "Password")]
    public void Validate_InvalidField_FailsWithCorrectProperty(
        string firstName, string lastName, string email, string password, string expectedProperty)
    {
        var request = new RegisterRequest(firstName, lastName, email, password);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(expectedProperty);
    }
}
