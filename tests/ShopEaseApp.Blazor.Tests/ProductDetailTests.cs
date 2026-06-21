using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ShopEaseApp.Api.Features.Catalog.Components;
using ShopEaseApp.Api.Features.Catalog.Products;
using ShopEaseApp.Api.Features.Cart;

namespace ShopEaseApp.Blazor.Tests;

public class ProductDetailTests
{
    // ── Scenario: Guest views an existing product's detail ─────────────────────

    [Fact]
    public async Task RendersProductAndVariants_WhenProductExists()
    {
        using var ctx = new TestContext();
        await using var db = TestHelpers.CreateDb();
        var (productId, _) = await TestHelpers.SeedProductAsync(db, "Gold Earring", "Accessories", 25.99m);
        var (cacheMock, _) = TestHelpers.CreateCache();

        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped<ProductHandler>();
        ctx.Services.AddScoped(_ => cacheMock.Object);
        ctx.Services.AddScoped<CartService>();

        var cut = ctx.RenderComponent<ProductDetail>(p => p.Add(x => x.Id, productId));

        Assert.Contains("Gold Earring", cut.Markup);
        Assert.Contains("Accessories", cut.Markup);
        Assert.Contains("Add to Cart", cut.Markup);
        Assert.Contains("25.99", cut.Markup); // variant price rendered
    }

    // ── Scenario: Product not found ─────────────────────────────────────────────

    [Fact]
    public async Task ShowsNotFoundMessage_WhenProductDoesNotExist()
    {
        using var ctx = new TestContext();
        await using var db = TestHelpers.CreateDb();
        var (cacheMock, _) = TestHelpers.CreateCache();

        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped<ProductHandler>();
        ctx.Services.AddScoped(_ => cacheMock.Object);
        ctx.Services.AddScoped<CartService>();

        var cut = ctx.RenderComponent<ProductDetail>(p => p.Add(x => x.Id, 999));

        Assert.Contains("Product not found", cut.Markup);
        Assert.Contains("Back to catalog", cut.Markup);
        Assert.Contains("/", cut.Markup);
    }
}
