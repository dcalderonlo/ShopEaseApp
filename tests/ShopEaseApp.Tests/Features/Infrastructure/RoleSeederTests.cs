using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ShopEaseApp.Api.Infrastructure.Data.Seeding;

namespace ShopEaseApp.Tests.Features.Infrastructure;

public class RoleSeederTests
{
    [Fact]
    public async Task SeedRolesAsync_CreatesCustomerAndAdminRoles_WhenTheyDoNotExist()
    {
        // Arrange
        var services = new ServiceCollection();

        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        roleStore
            .Setup(s => s.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityRole?)null);
        roleStore
            .Setup(s => s.CreateAsync(It.IsAny<IdentityRole>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Success);

        var roleManager = new RoleManager<IdentityRole>(
            roleStore.Object,
            [],
            new UpperInvariantLookupNormalizer(),
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<ILogger<RoleManager<IdentityRole>>>().Object);

        services.AddSingleton(roleManager);
        var provider = services.BuildServiceProvider();

        // Act
        await RoleSeeder.SeedRolesAsync(provider);

        // Assert — CreateAsync must have been called for both roles
        roleStore.Verify(
            s => s.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Customer"), It.IsAny<CancellationToken>()),
            Times.Once);
        roleStore.Verify(
            s => s.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Admin"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SeedRolesAsync_DoesNotDuplicateRoles_WhenTheyAlreadyExist()
    {
        // Arrange
        var services = new ServiceCollection();

        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        roleStore
            .Setup(s => s.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityRole("Existing")); // roles already exist

        var roleManager = new RoleManager<IdentityRole>(
            roleStore.Object,
            Array.Empty<IRoleValidator<IdentityRole>>(),
            new UpperInvariantLookupNormalizer(),
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<ILogger<RoleManager<IdentityRole>>>().Object);

        services.AddSingleton(roleManager);
        var provider = services.BuildServiceProvider();

        // Act
        await RoleSeeder.SeedRolesAsync(provider);

        // Assert — CreateAsync must NOT have been called
        roleStore.Verify(
            s => s.CreateAsync(It.IsAny<IdentityRole>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
