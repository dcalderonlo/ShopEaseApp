using FluentValidation;

namespace ShopEaseApp.Api.Features.Identity.ChangePassword;

/// <summary>
/// FluentValidation rules for <see cref="ChangePasswordRequest"/>.
/// Mirrors the Identity password policy minimum length (6 characters).
/// </summary>
public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
    }
}
