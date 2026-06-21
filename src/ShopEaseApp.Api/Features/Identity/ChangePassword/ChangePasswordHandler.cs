using Microsoft.AspNetCore.Identity;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Api.Features.Identity.ChangePassword;

/// <summary>
/// Handles an authenticated password change. Calls UserManager.ChangePasswordAsync
/// and, on success, atomically clears the MustChangePassword flag so the user is
/// not redirected to /change-password again (per the design decision).
/// </summary>
public class ChangePasswordHandler
{
    private readonly UserManager<AppUser> _userManager;

    public ChangePasswordHandler(UserManager<AppUser> userManager) => _userManager = userManager;

    public virtual async Task<(bool Success, string? Error)> HandleAsync(
        AppUser user, ChangePasswordRequest request)
    {
        var result = await _userManager.ChangePasswordAsync(
            user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
            return (false, result.Errors.First().Description);

        // Atomic clear: drop the forced-change flag in the same operation so an
        // orphaned flag cannot trap the user in a redirect loop.
        if (user.MustChangePassword)
        {
            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);
        }

        return (true, null);
    }
}
