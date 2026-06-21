using System.Security.Claims;
using Bunit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ShopEaseApp.Api.Domain;
using ShopEaseApp.Api.Features.Cart;
using ShopEaseApp.Api.Features.Identity.Login;
using ShopEaseApp.Api.Features.Identity.Register;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Blazor.Tests;

/// <summary>
/// Shared bUnit test infrastructure: InMemory DbContext, a Dictionary-backed
/// IDistributedCache (mirrors the existing ShopEaseApp.Tests pattern), auth
/// principals, and mockable Identity handlers (HandleAsync is virtual).
/// </summary>
internal static class TestHelpers
{
    public static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("BlazorTests_" + Guid.NewGuid())
            .Options;
        return new AppDbContext(options);
    }

    public static (Mock<IDistributedCache> mock, Dictionary<string, byte[]> store) CreateCache()
    {
        var store = new Dictionary<string, byte[]>();
        var mock = new Mock<IDistributedCache>();

        mock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, CancellationToken _) =>
                store.TryGetValue(key, out var v) ? v : null);

        mock.Setup(c => c.SetAsync(
                It.IsAny<string>(), It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns((string key, byte[] value, DistributedCacheEntryOptions _, CancellationToken _) =>
            {
                store[key] = value;
                return Task.CompletedTask;
            });

        mock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns((string key, CancellationToken _) =>
            {
                store.Remove(key);
                return Task.CompletedTask;
            });

        return (mock, store);
    }

    /// <summary>Seed a category + product + single variant. Returns variantId.</summary>
    public static async Task<(int productId, int variantId)> SeedProductAsync(
        AppDbContext db, string productName, string category, decimal price, int stock = 10)
    {
        var cat = new Category { Name = category };
        db.Categories.Add(cat);
        await db.SaveChangesAsync();

        var product = new Product
        {
            Name = productName,
            CategoryId = cat.Id,
            ImageUrls = [],
            Variants = [new() { Name = "Default", Price = price, Stock = stock }]
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return (product.Id, product.Variants.First().Id);
    }

    public static ClaimsPrincipal Authenticated(string userId, string email = "shopper@test", string role = "Customer")
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            },
            authenticationType: "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    public static ClaimsPrincipal Anonymous() =>
        new(new ClaimsIdentity());

    public static Mock<LoginHandler> MockLoginHandler(bool success, LoginResponse? response)
    {
        var mock = new Mock<LoginHandler>(null!, null!, null!) { CallBase = false };
        mock.Setup(h => h.HandleAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync((success, response));
        return mock;
    }

    public static Mock<RegisterHandler> MockRegisterHandler(
        bool success, RegisterResponse? response, IEnumerable<string> errors)
    {
        var mock = new Mock<RegisterHandler>(null!) { CallBase = false };
        mock.Setup(h => h.HandleAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync((success, response, errors));
        return mock;
    }
}

/// <summary>
/// AuthenticationStateProvider used in bUnit tests to feed an exact principal
/// (with specific claims like NameIdentifier) into the cascading auth state.
/// </summary>
internal sealed class FakeAuthStateProvider : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal _user;
    public FakeAuthStateProvider(ClaimsPrincipal user) => _user = user;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(_user));
}

internal static class AuthRenderExtensions
{
    /// <summary>
    /// Renders an auth-gated component inside a real
    /// <c>CascadingAuthenticationState</c>, backed by <see cref="FakeAuthStateProvider"/>
    /// with the supplied principal. Core authorization services are registered so
    /// <c>[Authorize]</c> / <c>AuthorizeView</c> can evaluate.
    /// </summary>
    public static IRenderedComponent<T> RenderWithAuth<T>(this TestContext ctx, ClaimsPrincipal user)
        where T : IComponent
    {
        // bUnit pre-registers a PlaceholderAuthorizationService for IAuthorizationService
        // that throws when <AuthorizeView> is rendered. AddAuthorizationCore uses TryAdd,
        // so it cannot override the placeholder — remove it first, then register the real
        // DefaultAuthorizationService so <AuthorizeView>/[Authorize] evaluate normally.
        foreach (var d in ctx.Services.Where(s => s.ServiceType == typeof(IAuthorizationService)).ToList())
            ctx.Services.Remove(d);

        ctx.Services.AddAuthorizationCore();
        ctx.Services.AddScoped<AuthenticationStateProvider>(_ => new FakeAuthStateProvider(user));
        var wrapper = ctx.RenderComponent<CascadingAuthenticationState>(
            parameters => parameters.AddChildContent<T>());
        return wrapper.FindComponent<T>();
    }
}
