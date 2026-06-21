using Microsoft.AspNetCore.Identity;
using ShopEaseApp.Api.Infrastructure.Auth;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Api.Features.Identity.Login;

public class LoginHandler(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    JwtService jwtService)
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly SignInManager<AppUser> _signInManager = signInManager;
    private readonly JwtService _jwtService = jwtService;

  public virtual async Task<(bool Success, LoginResponse? Response)> HandleAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return (false, null);

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return (false, null);

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);
        var expiry = _jwtService.GetExpiry();

        return (true, new LoginResponse(token, user.Email!, roles.FirstOrDefault() ?? "Customer", expiry, user.MustChangePassword));
    }
}
