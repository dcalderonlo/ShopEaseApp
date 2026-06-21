using Microsoft.AspNetCore.Identity;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Api.Infrastructure.Data.Seeding;

public static class AdminSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        const string adminEmail = "admin@shopease.com";
        const string adminPassword = "Admin123!";

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return; // admin already exists

        var admin = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Admin",
            CreatedAt = DateTime.UtcNow,
            MustChangePassword = true // force a password change on first login
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (!result.Succeeded) return;

        await userManager.AddToRoleAsync(admin, "Admin");
    }
}
