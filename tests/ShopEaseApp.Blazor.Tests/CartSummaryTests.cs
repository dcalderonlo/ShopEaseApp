using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ShopEaseApp.Api.Features.Cart;
using ShopEaseApp.Api.Features.Cart.Components;

namespace ShopEaseApp.Blazor.Tests;

public class CartSummaryTests
{
    // ── Scenario: Authenticated user's item count appears in the badge ──────────

    [Fact]
    public async Task ShowsItemCount_WhenAuthenticatedUserHasItems()
    {
        using var ctx = new TestContext();
        await using var db = TestHelpers.CreateDb();
        var (_, v1) = await TestHelpers.SeedProductAsync(db, "P1", "C", 10m);
        var (_, v2) = await TestHelpers.SeedProductAsync(db, "P2", "C", 15m);
        var (cacheMock, _) = TestHelpers.CreateCache();

        var seeder = new CartService(cacheMock.Object, db);
        await seeder.AddItemAsync("user-1", new AddItemRequest(v1, 2));
        await seeder.AddItemAsync("user-1", new AddItemRequest(v2, 1));

        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped(_ => cacheMock.Object);
        ctx.Services.AddScoped<CartService>();

        var cut = ctx.RenderWithAuth<CartSummary>(TestHelpers.Authenticated("user-1"));

        // 2 + 1 = 3 items total
        Assert.Contains("Cart (3)", cut.Markup);
    }

    // ── Scenario: Guest sees no count badge ─────────────────────────────────────

    [Fact]
    public async Task ShowsNoCount_WhenUserIsAnonymous()
    {
        using var ctx = new TestContext();
        await using var db = TestHelpers.CreateDb();
        var (cacheMock, _) = TestHelpers.CreateCache();

        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped(_ => cacheMock.Object);
        ctx.Services.AddScoped<CartService>();

        var cut = ctx.RenderWithAuth<CartSummary>(TestHelpers.Anonymous());

        Assert.DoesNotContain("Cart (", cut.Markup);
    }
}
