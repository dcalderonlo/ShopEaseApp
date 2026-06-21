using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ShopEaseApp.Api.Infrastructure.Data;
using ShopEaseApp.Api.Infrastructure.Data.Seeding;

namespace ShopEaseApp.Tests.Components;

public class AdminSeederTests
{
    [Fact]
    public async Task SeedAdminAsync_CreatesDefaultAdminWhenNoneExists()
    {
        // Arrange
        var services = new ServiceCollection();
        var userStore = new Mock<IUserStore<AppUser>>().Object;

        var userManager = new Mock<UserManager<AppUser>>(
            userStore, null!, null!, null!, null!, null!, null!, null!, null!);

        userManager.Setup(m => m.FindByEmailAsync("admin@shopease.com"))
            .ReturnsAsync((AppUser?)null); // admin doesn't exist yet
        userManager.Setup(m => m.CreateAsync(It.Is<AppUser>(u => u.Email == "admin@shopease.com"), "Admin123!"))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        services.AddSingleton(userManager.Object);
        var provider = services.BuildServiceProvider();

        // Act
        await AdminSeeder.SeedAdminAsync(provider);

        // Assert
        userManager.Verify(m => m.CreateAsync(
            It.Is<AppUser>(u => u.Email == "admin@shopease.com" && u.FirstName == "System"),
            "Admin123!"), Times.Once);
        userManager.Verify(m => m.AddToRoleAsync(It.IsAny<AppUser>(), "Admin"), Times.Once);
    }

    // ── Scenario: Seeded admin is flagged for a forced first password change ──

    [Fact]
    public async Task SeedAdminAsync_SetsMustChangePasswordTrueOnSeededAdmin()
    {
        // Arrange
        var services = new ServiceCollection();
        var userStore = new Mock<IUserStore<AppUser>>().Object;

        var userManager = new Mock<UserManager<AppUser>>(
            userStore, null!, null!, null!, null!, null!, null!, null!, null!);

        userManager.Setup(m => m.FindByEmailAsync("admin@shopease.com"))
            .ReturnsAsync((AppUser?)null); // admin doesn't exist yet
        userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), "Admin123!"))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        services.AddSingleton(userManager.Object);
        var provider = services.BuildServiceProvider();

        // Act
        await AdminSeeder.SeedAdminAsync(provider);

        // Assert — the seeded admin must be flagged for first-change
        userManager.Verify(m => m.CreateAsync(
            It.Is<AppUser>(u => u.MustChangePassword == true), "Admin123!"), Times.Once);
    }

    [Fact]
    public async Task SeedAdminAsync_SkipsWhenAdminAlreadyExists()
    {
        // Arrange
        var services = new ServiceCollection();
        var userStore = new Mock<IUserStore<AppUser>>().Object;

        var userManager = new Mock<UserManager<AppUser>>(
            userStore, null!, null!, null!, null!, null!, null!, null!, null!);

        userManager.Setup(m => m.FindByEmailAsync("admin@shopease.com"))
            .ReturnsAsync(new AppUser { Email = "admin@shopease.com" }); // already exists

        services.AddSingleton(userManager.Object);
        var provider = services.BuildServiceProvider();

        // Act
        await AdminSeeder.SeedAdminAsync(provider);

        // Assert
        userManager.Verify(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
    }
}
