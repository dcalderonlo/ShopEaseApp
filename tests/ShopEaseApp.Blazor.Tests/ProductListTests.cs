using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ShopEaseApp.Api.Features.Catalog.Components;
using ShopEaseApp.Api.Features.Catalog.Products;

namespace ShopEaseApp.Blazor.Tests;

public class ProductListTests
{
    // ── Scenario: Guest browses product catalog ────────────────────────────────

    [Fact]
    public async Task RendersProductsFromHandler()
    {
        using var ctx = new TestContext();
        await using var db = TestHelpers.CreateDb();
        await TestHelpers.SeedProductAsync(db, "Gold Earring", "Accessories", 25.99m);
        await TestHelpers.SeedProductAsync(db, "Silver Ring", "Jewelry", 49.50m);

        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped<ProductHandler>();

        var cut = ctx.RenderComponent<ProductList>();

        // Each product name + category rendered from real handler → real db data
        Assert.Contains("Gold Earring", cut.Markup);
        Assert.Contains("Silver Ring", cut.Markup);
        Assert.Contains("Accessories", cut.Markup);
        // Link to the product detail page is rendered
        Assert.Contains("/product/", cut.Markup);
    }

    // ── Scenario: Catalog with no products ─────────────────────────────────────

    [Fact]
    public async Task ShowsEmptyMessage_WhenNoProducts()
    {
        using var ctx = new TestContext();
        await using var db = TestHelpers.CreateDb();

        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped<ProductHandler>();

        var cut = ctx.RenderComponent<ProductList>();

        Assert.Contains("No products available", cut.Markup);
    }
}
