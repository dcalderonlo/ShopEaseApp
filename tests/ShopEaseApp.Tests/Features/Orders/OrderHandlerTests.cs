using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using ShopEaseApp.Api.Domain;
using ShopEaseApp.Api.Features.Cart;
using ShopEaseApp.Api.Features.Orders;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Tests.Features.Orders;

public class OrderHandlerTests
{
    // ── Test infrastructure ───────────────────────────────────────────────────

    private static AppDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("OrderTests_" + Guid.NewGuid())
            .Options;
        return new AppDbContext(opts);
    }

    private static (Mock<IDistributedCache> mock, Dictionary<string, byte[]> store) CreateCache()
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

    private static void SeedCart(Dictionary<string, byte[]> store, string userId,
        IEnumerable<CartItem> items)
    {
        store[$"cart:{userId}"] =
            System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(items.ToList()));
    }

    private static async Task<(int catId, int productId, int variantId)> SeedCatalogAsync(
        AppDbContext db, int stock = 10, decimal price = 25m)
    {
        var cat = new Category { Name = "Accessories" };
        db.Categories.Add(cat);
        await db.SaveChangesAsync();

        var product = new Product
        {
            Name = "Gold Earring", CategoryId = cat.Id, ImageUrls = []
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var variant = new ProductVariant
        {
            ProductId = product.Id, Name = "Gold", Price = price, Stock = stock
        };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync();

        return (cat.Id, product.Id, variant.Id);
    }

    // ── Scenario: Customer checks out successfully ────────────────────────────

    [Fact]
    public async Task CreateFromCartAsync_CreatesConfirmedOrderAndDecrementsStock()
    {
        await using var db = CreateDb();
        var (catId, productId, variantId) = await SeedCatalogAsync(db, stock: 5, price: 30m);
        var (cacheMock, store) = CreateCache();
        SeedCart(store, "user1",
        [
            new CartItem { VariantId = variantId, VariantName = "Gold",
                           ProductName = "Gold Earring", Quantity = 2, PriceSnapshot = 30m }
        ]);

        var cartService = new CartService(cacheMock.Object, db);
        var handler = new OrderHandler(db, cartService);

        var (success, order, error) = await handler.CreateFromCartAsync("user1");

        // Order created with correct status and total
        Assert.True(success);
        Assert.NotNull(order);
        Assert.Equal("Confirmed", order!.Status);
        Assert.Equal(60m, order.Total);   // 2 × 30
        Assert.Single(order.Items);

        // Stock decremented
        var variant = await db.ProductVariants.FindAsync(variantId);
        Assert.Equal(3, variant!.Stock);  // 5 - 2

        // Cart cleared
        var cart = await cartService.GetCartAsync("user1");
        Assert.Empty(cart.Items);
    }

    // ── Scenario: Checkout rejected — insufficient stock ─────────────────────

    [Fact]
    public async Task CreateFromCartAsync_RejectsOrderWhenStockInsufficient()
    {
        await using var db = CreateDb();
        var (_, _, variantId) = await SeedCatalogAsync(db, stock: 1, price: 20m);
        var (cacheMock, store) = CreateCache();
        SeedCart(store, "user2",
        [
            new CartItem { VariantId = variantId, VariantName = "Gold",
                           ProductName = "Earring", Quantity = 5, PriceSnapshot = 20m }
        ]);

        var cartService = new CartService(cacheMock.Object, db);
        var handler = new OrderHandler(db, cartService);

        var (success, order, error) = await handler.CreateFromCartAsync("user2");

        Assert.False(success);
        Assert.Null(order);
        Assert.NotNull(error);
        Assert.Contains("Insufficient stock", error);

        // Stock NOT decremented
        var variant = await db.ProductVariants.FindAsync(variantId);
        Assert.Equal(1, variant!.Stock);

        // Cart NOT cleared
        var cart = await cartService.GetCartAsync("user2");
        Assert.Single(cart.Items);
    }

    // ── Scenario: Mixed cart — one item fails, entire order rejected ──────────

    [Fact]
    public async Task CreateFromCartAsync_RejectsEntireOrderIfAnyItemLacksStock()
    {
        await using var db = CreateDb();
        var (_, _, variantId1) = await SeedCatalogAsync(db, stock: 10, price: 15m);
        var (_, _, variantId2) = await SeedCatalogAsync(db, stock: 0, price: 20m); // out of stock

        var (cacheMock, store) = CreateCache();
        SeedCart(store, "user3",
        [
            new CartItem { VariantId = variantId1, VariantName = "A",
                           ProductName = "P1", Quantity = 1, PriceSnapshot = 15m },
            new CartItem { VariantId = variantId2, VariantName = "B",
                           ProductName = "P2", Quantity = 1, PriceSnapshot = 20m }
        ]);

        var cartService = new CartService(cacheMock.Object, db);
        var handler = new OrderHandler(db, cartService);

        var (success, order, error) = await handler.CreateFromCartAsync("user3");

        Assert.False(success);

        // Neither item's stock was decremented
        var v1 = await db.ProductVariants.FindAsync(variantId1);
        var v2 = await db.ProductVariants.FindAsync(variantId2);
        Assert.Equal(10, v1!.Stock);
        Assert.Equal(0, v2!.Stock);
    }

    // ── Scenario: Create from empty cart ─────────────────────────────────────

    [Fact]
    public async Task CreateFromCartAsync_RejectsWhenCartEmpty()
    {
        await using var db = CreateDb();
        var (cacheMock, _) = CreateCache();
        var cartService = new CartService(cacheMock.Object, db);
        var handler = new OrderHandler(db, cartService);

        var (success, order, error) = await handler.CreateFromCartAsync("user-empty");

        Assert.False(success);
        Assert.Equal("Cart is empty.", error);
    }

    // ── Scenario: Customer views owned order ─────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsOrderForOwner()
    {
        await using var db = CreateDb();
        var order = new Order
        {
            CustomerId = "owner-1",
            Status = OrderStatus.Confirmed,
            Total = 50m,
            CreatedAt = DateTime.UtcNow,
            Items =
            [
                new() { VariantId = 1, VariantName = "X", ProductName = "P",
                         Quantity = 2, UnitPrice = 25m }
            ]
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var (cacheMock, _) = CreateCache();
        var cartService = new CartService(cacheMock.Object, db);
        var handler = new OrderHandler(db, cartService);

        var result = await handler.GetByIdAsync(order.Id, "owner-1");

        Assert.NotNull(result);
        Assert.Equal(50m, result!.Total);
        Assert.Equal("Confirmed", result.Status);
    }

    // ── Scenario: Customer requests another customer's order ──────────────────

    [Fact]
    public async Task GetByIdAsync_DeniesAccessToOtherCustomerOrder()
    {
        await using var db = CreateDb();
        var order = new Order
        {
            CustomerId = "owner-A",
            Status = OrderStatus.Confirmed,
            Total = 100m,
            CreatedAt = DateTime.UtcNow
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var (cacheMock, _) = CreateCache();
        var cartService = new CartService(cacheMock.Object, db);
        var handler = new OrderHandler(db, cartService);

        // Different user tries to access owner-A's order
        var result = await handler.GetByIdAsync(order.Id, "another-user");

        Assert.Null(result);
    }

    // ── Scenario: Customer views history with no orders ───────────────────────

    [Fact]
    public async Task GetCustomerHistoryAsync_ReturnsEmptyForNewCustomer()
    {
        await using var db = CreateDb();
        var (cacheMock, _) = CreateCache();
        var cartService = new CartService(cacheMock.Object, db);
        var handler = new OrderHandler(db, cartService);

        var history = await handler.GetCustomerHistoryAsync("new-user");

        Assert.Empty(history);
    }
}
