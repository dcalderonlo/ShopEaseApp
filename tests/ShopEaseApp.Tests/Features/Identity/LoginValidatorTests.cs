using FluentValidation.TestHelper;
using ShopEaseApp.Api.Features.Identity.Login;

namespace ShopEaseApp.Tests.Features.Identity;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_PassesAllRules()
    {
        var request = new LoginRequest("user@example.com", "Password1!");
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("not-an-email", "Password1!", "Email")]
    [InlineData("", "Password1!", "Email")]
    [InlineData("user@example.com", "", "Password")]
    public void Validate_InvalidField_FailsWithCorrectProperty(
        string email, string password, string expectedProperty)
    {
        var request = new LoginRequest(email, password);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(expectedProperty);
    }
}
