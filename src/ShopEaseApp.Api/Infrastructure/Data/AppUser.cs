using Microsoft.AspNetCore.Identity;

namespace ShopEaseApp.Api.Infrastructure.Data;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When true, the user must change their password before accessing protected
    /// features (forced first-change flow for seeded admin accounts).
    /// </summary>
    public bool MustChangePassword { get; set; }
}
