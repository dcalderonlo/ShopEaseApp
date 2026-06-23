using Microsoft.EntityFrameworkCore;
using ShopEaseApp.Api.Domain;
using ShopEaseApp.Api.Features.Admin.Dashboard;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Tests.Features.Admin.Dashboard;

public class AdminDashboardHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("AdminDashboardTests_" + Guid.NewGuid())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task SeedAsync(AppDbContext db)
    {
        var cat = new Category { Name = "Jewelry" };
        db.Categories.Add(cat);
        await db.SaveChangesAsync();

        // Product A — one variant, In Stock
        db.Products.Add(new Product
        {
            Name = "Alpha Ring",
            CategoryId = cat.Id,
            ImageUrls = [],
            Variants = [new() { Name = "Gold", Price = 100m, Stock = 10, MinimumStockLevel = 5 }]
        });

        // Product B — two variants: one Low Stock, one Out of Stock
        db.Products.Add(new Product
        {
            Name = "Beta Necklace",
            CategoryId = cat.Id,
            ImageUrls = [],
            Variants =
            [
                new() { Name = "Silver", Price = 50m, Stock = 3, MinimumStockLevel = 5 },   // Low Stock
                new() { Name = "Platinum", Price = 200m, Stock = 0, MinimumStockLevel = 5 } // Out of Stock
            ]
        });

        await db.SaveChangesAsync();
    }

    // ── Scenario: Admin retrieves dashboard metrics ────────────────────────────
    // 2 products, 3 SKUs, 1 low-stock item (Silver: stock 3 <= min 5), inventory = 1000+150+0

    [Fact]
    public async Task GetMetricsAsync_ReturnsAccurateTotals()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var handler = new AdminDashboardHandler(db);

        var metrics = await handler.GetMetricsAsync();

        Assert.Equal(2, metrics.TotalProducts);
        Assert.Equal(3, metrics.TotalSkus);
        Assert.Equal(1, metrics.LowStockItems);
        Assert.Equal(1150m, metrics.InventoryValue);
    }

    // ── Scenario: Empty catalog yields zero metrics (triangulation) ────────────

    [Fact]
    public async Task GetMetricsAsync_ReturnsZeros_WhenCatalogEmpty()
    {
        await using var db = CreateDb();
        var handler = new AdminDashboardHandler(db);

        var metrics = await handler.GetMetricsAsync();

        Assert.Equal(0, metrics.TotalProducts);
        Assert.Equal(0, metrics.TotalSkus);
        Assert.Equal(0, metrics.LowStockItems);
        Assert.Equal(0m, metrics.InventoryValue);
    }

    // ── Scenario: Admin browses product list (no filter) ────────────────────────

    [Fact]
    public async Task GetProductsAsync_ReturnsAllVariantsWithComputedStatus()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var handler = new AdminDashboardHandler(db);

        var products = await handler.GetProductsAsync();

        Assert.Equal(3, products.Count);
        // Ordered by ProductName → "Alpha Ring" first
        Assert.Equal("Alpha Ring", products[0].ProductName);
        Assert.Equal("In Stock", products[0].Status);
        Assert.Equal("Low Stock", products.Single(p => p.VariantName == "Silver").Status);
        Assert.Equal("Out of Stock", products.Single(p => p.VariantName == "Platinum").Status);
        Assert.Equal("Jewelry", products[0].CategoryName);
    }

    // ── Scenario: Admin filters by stock status ────────────────────────────────

    [Theory]
    [InlineData("Low Stock", "Silver")]
    [InlineData("Out of Stock", "Platinum")]
    [InlineData("In Stock", "Gold")]
    public async Task GetProductsAsync_FiltersByStatus(string filter, string expectedVariantName)
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var handler = new AdminDashboardHandler(db);

        var products = await handler.GetProductsAsync(filter);

        var single = Assert.Single(products);
        Assert.Equal(expectedVariantName, single.VariantName);
        Assert.Equal(filter, single.Status);
    }

    // ── Scenario: MinimumStockLevel varies per variant ─────────────────────────
    // Same Stock=4, different MinimumStockLevel → different status.

    [Fact]
    public async Task GetProductsAsync_StatusRespectsPerVariantMinimumStockLevel()
    {
        await using var db = CreateDb();
        var cat = new Category { Name = "Misc" };
        db.Categories.Add(cat);
        await db.SaveChangesAsync();

        db.Products.Add(new Product
        {
            Name = "Mixed",
            CategoryId = cat.Id,
            ImageUrls = [],
            Variants =
            [
                new() { Name = "LowMin", Price = 10m, Stock = 4, MinimumStockLevel = 3 },  // In Stock
                new() { Name = "HighMin", Price = 10m, Stock = 4, MinimumStockLevel = 5 } // Low Stock
            ]
        });
        await db.SaveChangesAsync();

        var handler = new AdminDashboardHandler(db);
        var products = await handler.GetProductsAsync();

        Assert.Equal("In Stock", products.Single(p => p.VariantName == "LowMin").Status);
        Assert.Equal("Low Stock", products.Single(p => p.VariantName == "HighMin").Status);
    }
}
