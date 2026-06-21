using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ShopEaseApp.Api.Features.Cart;
using ShopEaseApp.Api.Features.Cart.Components;
using ShopEaseApp.Api.Features.Orders;

namespace ShopEaseApp.Blazor.Tests;

public class CheckoutTests
{
    // ── Scenario: Empty cart checkout attempt shows message ────────────────────

    [Fact]
    public async Task ShowsEmptyMessage_WhenCartIsEmpty()
    {
        using var ctx = new TestContext();
        await using var db = TestHelpers.CreateDb();
        var (cacheMock, _) = TestHelpers.CreateCache();

        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped(_ => cacheMock.Object);
        ctx.Services.AddScoped<CartService>();
        ctx.Services.AddScoped<OrderHandler>();

        var cut = ctx.RenderWithAuth<Checkout>(TestHelpers.Authenticated("user-empty"));

        Assert.Contains("Your cart is empty", cut.Markup);
        Assert.Contains("Back to cart", cut.Markup);
    }

    // ── Scenario: Successful checkout creates order and shows confirmation ─────

    [Fact]
    public async Task ConfirmCheckout_CreatesOrderAndShowsOrderNumber()
    {
        using var ctx = new TestContext();
        await using var db = TestHelpers.CreateDb();
        var (_, variantId) = await TestHelpers.SeedProductAsync(db, "Checkout Product", "C", 20m, stock: 5);
        var (cacheMock, _) = TestHelpers.CreateCache();

        var seeder = new CartService(cacheMock.Object, db);
        await seeder.AddItemAsync("user-1", new AddItemRequest(variantId, 2));

        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped(_ => cacheMock.Object);
        ctx.Services.AddScoped<CartService>();
        ctx.Services.AddScoped<OrderHandler>();

        var cut = ctx.RenderWithAuth<Checkout>(TestHelpers.Authenticated("user-1"));

        // Before confirm: button + total visible
        Assert.Contains("Confirm Checkout", cut.Markup);
        cut.Find("button").Click();

        // After confirm: confirmation + order number rendered
        Assert.Contains("Order confirmed", cut.Markup);
        Assert.Contains("order number", cut.Markup.ToLower());
        Assert.Contains("#", cut.Markup);
    }
}
