using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ShopEaseApp.Api.Features.Identity.Register;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Tests.Features.Identity;

public class RegisterHandlerTests
{
    private static UserManager<AppUser> BuildUserManager(
        Mock<IUserStore<AppUser>>? storeMock = null)
    {
        var store = storeMock ?? new Mock<IUserStore<AppUser>>();
        return new UserManager<AppUser>(
            store.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<AppUser>>().Object,
            [],
            [],
            new UpperInvariantLookupNormalizer(),
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<AppUser>>>().Object);
    }

    // ── Scenario: Successful customer registration ────────────────────────────

    [Fact]
    public async Task HandleAsync_ValidRequest_CreatesUserWithCustomerRole()
    {
        // RED: test written before handler existed
        var storeMock = new Mock<IUserStore<AppUser>>();
        storeMock.As<IUserEmailStore<AppUser>>()
            .Setup(s => s.SetEmailAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var userManagerMock = new Mock<UserManager<AppUser>>(
            storeMock.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<AppUser>>().Object,
            Array.Empty<IUserValidator<AppUser>>(),
            Array.Empty<IPasswordValidator<AppUser>>(),
            new UpperInvariantLookupNormalizer(),
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<AppUser>>>().Object);

        userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), "Customer"))
            .ReturnsAsync(IdentityResult.Success);

        var handler = new RegisterHandler(userManagerMock.Object);
        var request = new RegisterRequest("Jane", "Doe", "jane@example.com", "Password1!");

        // Act
        var (success, response, errors) = await handler.HandleAsync(request);

        // Assert
        Assert.True(success);
        Assert.NotNull(response);
        Assert.Equal("Customer", response.Role);
        Assert.Equal("jane@example.com", response.Email);
        userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<AppUser>(), "Customer"), Times.Once);
    }

    // ── Scenario: Registration with duplicate/invalid credentials ─────────────

    [Fact]
    public async Task HandleAsync_DuplicateEmail_ReturnsFalseWithErrors()
    {
        var userManagerMock = new Mock<UserManager<AppUser>>(
            new Mock<IUserStore<AppUser>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<AppUser>>().Object,
            Array.Empty<IUserValidator<AppUser>>(),
            Array.Empty<IPasswordValidator<AppUser>>(),
            new UpperInvariantLookupNormalizer(),
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<AppUser>>>().Object);

        userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email already taken." }));

        var handler = new RegisterHandler(userManagerMock.Object);
        var request = new RegisterRequest("Jane", "Doe", "existing@example.com", "Password1!");

        var (success, response, errors) = await handler.HandleAsync(request);

        Assert.False(success);
        Assert.Null(response);
        Assert.Contains("Email already taken.", errors);
    }
}
