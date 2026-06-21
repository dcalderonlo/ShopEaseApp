using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ShopEaseApp.Api.Features.Cart;
using ShopEaseApp.Api.Features.Cart.Components;

namespace ShopEaseApp.Blazor.Tests;

public class CartPageTests
{
    // ── Scenario: Authenticated user views a populated cart ─────────────────────

    [Fact]
    public async Task RendersCartItemsAndTotal_WhenCartHasItems()
    {
        using var ctx = new TestContext();
        await using var db = TestHelpers.CreateDb();
        var (_, variantId) = await TestHelpers.SeedProductAsync(db, "Test Product", "Accessories", 29.99m);
        var (cacheMock, _) = TestHelpers.CreateCache();

        // Pre-seed the cart for "user-1" using the real CartService
        var seeder = new CartService(cacheMock.Object, db);
        await seeder.AddItemAsync("user-1", new AddItemRequest(variantId, 2));

        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped(_ => cacheMock.Object);
        ctx.Services.AddScoped<CartService>();

        var cut = ctx.RenderWithAuth<CartPage>(TestHelpers.Authenticated("user-1"));

        Assert.Contains("Test Product", cut.Markup);
        Assert.Contains("Default", cut.Markup);          // variant name
        Assert.Contains("Checkout", cut.Markup);
        // Total = 2 × 29.99 = 59.98
        Assert.Contains("59.98", cut.Markup);
    }

    // ── Scenario: Empty cart shows empty message ────────────────────────────────

    [Fact]
    public async Task ShowsEmptyMessage_WhenCartIsEmpty()
    {
        using var ctx = new TestContext();
        await using var db = TestHelpers.CreateDb();
        var (cacheMock, _) = TestHelpers.CreateCache();

        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped(_ => cacheMock.Object);
        ctx.Services.AddScoped<CartService>();

        var cut = ctx.RenderWithAuth<CartPage>(TestHelpers.Authenticated("user-empty"));

        Assert.Contains("Your cart is empty", cut.Markup);
    }

    // ── Scenario: Remove item updates the rendered cart ─────────────────────────

    [Fact]
    public async Task RemoveItem_RemovesLineFromRenderedCart()
    {
        using var ctx = new TestContext();
        await using var db = TestHelpers.CreateDb();
        var (_, variantId) = await TestHelpers.SeedProductAsync(db, "Removable Product", "Cat", 10m);
        var (cacheMock, _) = TestHelpers.CreateCache();

        var seeder = new CartService(cacheMock.Object, db);
        await seeder.AddItemAsync("user-2", new AddItemRequest(variantId, 1));

        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped(_ => cacheMock.Object);
        ctx.Services.AddScoped<CartService>();

        var cut = ctx.RenderWithAuth<CartPage>(TestHelpers.Authenticated("user-2"));

        Assert.Contains("Removable Product", cut.Markup);
        cut.FindAll("button").First(b => b.TextContent == "Remove").Click();

        Assert.DoesNotContain("Removable Product", cut.Markup);
        Assert.Contains("Your cart is empty", cut.Markup);
    }
}
