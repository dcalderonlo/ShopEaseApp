using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using ShopEaseApp.Api.Domain;
using ShopEaseApp.Api.Features.Cart;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Tests.Features.Cart;

public class CartServiceTests
{
    // ── Test infrastructure ───────────────────────────────────────────────────

    private static AppDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("CartTests_" + Guid.NewGuid())
            .Options;
        return new AppDbContext(opts);
    }

    private static (Mock<IDistributedCache> cacheMock, Dictionary<string, byte[]> store) CreateCache()
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

    private static async Task<int> SeedVariantAsync(AppDbContext db, decimal price = 29.99m)
    {
        var cat = new Category { Name = "Test Category" };
        db.Categories.Add(cat);
        var product = new Product
        {
            Name = "Test Product",
            CategoryId = 0,
            ImageUrls = new List<string>()
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        product.CategoryId = cat.Id;
        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "Default",
            Price = price,
            Stock = 10
        };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync();
        return variant.Id;
    }

    // ── Scenario: View a populated cart ──────────────────────────────────────

    [Fact]
    public async Task GetCartAsync_ReturnsItemsForUser()
    {
        await using var db = CreateDb();
        var (cacheMock, store) = CreateCache();

        var items = new List<CartItem>
        {
            new() { VariantId = 1, VariantName = "Red", ProductName = "Bracelet", Quantity = 2, PriceSnapshot = 15m }
        };
        store["cart:user1"] = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(items));

        var service = new CartService(cacheMock.Object, db);
        var cart = await service.GetCartAsync("user1");

        Assert.Single(cart.Items);
        Assert.Equal(30m, cart.Total); // 2 × 15
        Assert.Equal("user1", cart.UserId);
    }

    // ── Scenario: Add a variant to the cart ──────────────────────────────────

    [Fact]
    public async Task AddItemAsync_AddsVariantWithPriceSnapshot()
    {
        await using var db = CreateDb();
        var variantId = await SeedVariantAsync(db, price: 29.99m);
        var (cacheMock, _) = CreateCache();

        var service = new CartService(cacheMock.Object, db);
        var (success, cart, error) = await service.AddItemAsync("user42",
            new AddItemRequest(variantId, 3));

        Assert.True(success);
        Assert.NotNull(cart);
        Assert.Single(cart!.Items);
        Assert.Equal(29.99m, cart.Items.First().PriceSnapshot);
        Assert.Equal(3, cart.Items.First().Quantity);
        Assert.Equal(89.97m, cart.Total); // 3 × 29.99
    }

    // ── Scenario: Reject a non-existent variant ───────────────────────────────

    [Fact]
    public async Task AddItemAsync_ReturnsFalseForUnknownVariant()
    {
        await using var db = CreateDb();
        var (cacheMock, _) = CreateCache();

        var service = new CartService(cacheMock.Object, db);
        var (success, cart, error) = await service.AddItemAsync("user1", new AddItemRequest(999, 1));

        Assert.False(success);
        Assert.Null(cart);
        Assert.Equal("Variant not found.", error);
    }

    // ── Scenario: Update item quantity ────────────────────────────────────────

    [Fact]
    public async Task UpdateItemAsync_ChangesQuantityPreservesPriceSnapshot()
    {
        await using var db = CreateDb();
        var (cacheMock, store) = CreateCache();

        var items = new List<CartItem>
        {
            new() { VariantId = 5, VariantName = "L", ProductName = "Shirt", Quantity = 1, PriceSnapshot = 20m }
        };
        store["cart:user2"] = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(items));

        var service = new CartService(cacheMock.Object, db);
        var (success, cart, _) = await service.UpdateItemAsync("user2", new UpdateItemRequest(5, 4));

        Assert.True(success);
        Assert.Equal(4, cart!.Items.First().Quantity);
        Assert.Equal(20m, cart.Items.First().PriceSnapshot); // snapshot unchanged
        Assert.Equal(80m, cart.Total);
    }

    // ── Scenario: Remove one cart item ────────────────────────────────────────

    [Fact]
    public async Task RemoveItemAsync_RemovesOnlyTargetVariant()
    {
        await using var db = CreateDb();
        var (cacheMock, store) = CreateCache();

        var items = new List<CartItem>
        {
            new() { VariantId = 1, VariantName = "A", ProductName = "P1", Quantity = 1, PriceSnapshot = 10m },
            new() { VariantId = 2, VariantName = "B", ProductName = "P2", Quantity = 2, PriceSnapshot = 20m }
        };
        store["cart:user3"] = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(items));

        var service = new CartService(cacheMock.Object, db);
        var cart = await service.RemoveItemAsync("user3", variantId: 1);

        Assert.Single(cart.Items);
        Assert.Equal(2, cart.Items.First().VariantId);
    }

    // ── Scenario: Clear a populated cart ─────────────────────────────────────

    [Fact]
    public async Task ClearAsync_EmptiesCart()
    {
        await using var db = CreateDb();
        var (cacheMock, store) = CreateCache();

        var items = new List<CartItem>
        {
            new() { VariantId = 7, VariantName = "X", ProductName = "Prod", Quantity = 1, PriceSnapshot = 5m }
        };
        store["cart:user4"] = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(items));

        var service = new CartService(cacheMock.Object, db);
        await service.ClearAsync("user4");

        var cart = await service.GetCartAsync("user4");
        Assert.Empty(cart.Items);
        Assert.Equal(0m, cart.Total);
    }

    // ── Scenario: Cart isolation between users ────────────────────────────────

    [Fact]
    public async Task GetCartAsync_IsolatesCartsByUserId()
    {
        await using var db = CreateDb();
        var (cacheMock, store) = CreateCache();

        var user1Items = new List<CartItem>
        {
            new() { VariantId = 10, VariantName = "Gold", ProductName = "Ring", Quantity = 1, PriceSnapshot = 100m }
        };
        store["cart:user-A"] = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(user1Items));
        // user-B has no cart

        var service = new CartService(cacheMock.Object, db);

        var cartA = await service.GetCartAsync("user-A");
        var cartB = await service.GetCartAsync("user-B");

        Assert.Single(cartA.Items);
        Assert.Empty(cartB.Items);
    }
}
