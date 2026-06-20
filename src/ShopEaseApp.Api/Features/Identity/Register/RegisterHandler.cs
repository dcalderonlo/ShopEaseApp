using Microsoft.AspNetCore.Identity;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Api.Features.Identity.Register;

public class RegisterHandler(UserManager<AppUser> userManager)
{
    private readonly UserManager<AppUser> _userManager = userManager;

  public async Task<(bool Success, RegisterResponse? Response, IEnumerable<string> Errors)> HandleAsync(
        RegisterRequest request)
    {
        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return (false, null, result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, "Customer");

        return (true, new RegisterResponse(user.Id, user.Email!, "Customer"), []);
    }
}
