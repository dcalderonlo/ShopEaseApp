using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ShopEaseApp.Api.Domain;
using ShopEaseApp.Api.Features.Cart;
using ShopEaseApp.Api.Features.Orders;
using ShopEaseApp.Api.Features.Orders.Components;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Blazor.Tests;

public class OrderHistoryTests
{
    private static async Task SeedOrderAsync(AppDbContext db, string customerId, decimal total, OrderStatus status)
    {
        db.Orders.Add(new Order
        {
            CustomerId = customerId,
            Status = status,
            Total = total,
            CreatedAt = DateTime.UtcNow,
            Items = [new() { VariantId = 1, VariantName = "X", ProductName = "P", Quantity = 1, UnitPrice = total }]
        });
        await db.SaveChangesAsync();
    }

    // ── Scenario: User views order history ─────────────────────────────────────

    [Fact]
    public async Task ListsOrders_WhenUserHasOrders()
    {
        using var ctx = new TestContext();
        await using var db = TestHelpers.CreateDb();
        await SeedOrderAsync(db, "user-1", 50m, OrderStatus.Confirmed);
        var (cacheMock, _) = TestHelpers.CreateCache();

        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped(_ => cacheMock.Object);
        ctx.Services.AddScoped<CartService>();
        ctx.Services.AddScoped<OrderHandler>();

        var cut = ctx.RenderWithAuth<OrderHistory>(TestHelpers.Authenticated("user-1"));

        Assert.Contains("Confirmed", cut.Markup);
        Assert.Contains("50.00", cut.Markup); // total rendered
        Assert.Contains("#", cut.Markup);     // order number prefix
    }

    // ── Scenario: No orders yet ─────────────────────────────────────────────────

    [Fact]
    public async Task ShowsNoOrdersMessage_WhenUserHasNoOrders()
    {
        using var ctx = new TestContext();
        await using var db = TestHelpers.CreateDb();
        var (cacheMock, _) = TestHelpers.CreateCache();

        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped(_ => cacheMock.Object);
        ctx.Services.AddScoped<CartService>();
        ctx.Services.AddScoped<OrderHandler>();

        var cut = ctx.RenderWithAuth<OrderHistory>(TestHelpers.Authenticated("user-none"));

        Assert.Contains("no orders", cut.Markup);
        Assert.Contains("Browse catalog", cut.Markup);
    }
}
