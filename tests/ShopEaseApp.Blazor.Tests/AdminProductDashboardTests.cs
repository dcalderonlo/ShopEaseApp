using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ShopEaseApp.Api.Components.DesignSystem.Admin;
using ShopEaseApp.Api.Features.Admin.Dashboard;
using ShopEaseApp.Api.Features.Catalog.Categories;
using ShopEaseApp.Api.Features.Catalog.Products;

namespace ShopEaseApp.Blazor.Tests;

public class AdminDashboardTests
{
    // 3 distinct products, one variant each, one Low Stock.
    private static DashboardResponse Metrics => new(3, 3, 1, 1150m);

    private static List<AdminProductResponse> AllProducts =>
    [
        new(1, "Alpha Ring", "Jewelry", 10, "Gold", 10, 5, "In Stock", 100m),
        new(2, "Beta Necklace", "Jewelry", 11, "Silver", 3, 5, "Low Stock", 50m),
        new(3, "Gamma Bracelet", "Jewelry", 12, "Platinum", 0, 5, "Out of Stock", 200m)
    ];

    private static Mock<AdminDashboardHandler> CreateHandlerMock()
    {
        var mock = new Mock<AdminDashboardHandler>(null!) { CallBase = false };
        mock.Setup(h => h.GetMetricsAsync()).ReturnsAsync(Metrics);
        mock.Setup(h => h.GetProductsAsync(It.IsAny<string?>()))
            .Returns<string?>(filter => Task.FromResult(
                string.IsNullOrEmpty(filter)
                    ? AllProducts
                    : AllProducts.Where(p => p.Status == filter).ToList()));
        return mock;
    }

    // The page always renders the modals (gated by a Visible flag), so their
    // injected handlers must be resolvable even when hidden. Register real
    // handlers over an in-memory DbContext, matching the CartPageTests pattern.
    private static void RegisterModalDependencies(TestContext ctx)
    {
        var db = TestHelpers.CreateDb();
        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped<ProductHandler>();
        ctx.Services.AddScoped<CategoryHandler>();
    }

    // ── Scenario: Admin dashboard renders summary cards from metrics ───────────

    [Fact]
    public void RendersSummaryCards_FromMetrics()
    {
        using var ctx = new TestContext();
        RegisterModalDependencies(ctx);
        ctx.Services.AddScoped(_ => CreateHandlerMock().Object);

        var cut = ctx.RenderWithAuth<AdminProduct>(
            TestHelpers.Authenticated("admin1", "admin@test", "Admin"));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Total Products", cut.Markup);
            Assert.Contains("3 SKUs", cut.Markup);
            Assert.Contains("Low Stock Items", cut.Markup);
            Assert.Contains("Total Valuation", cut.Markup);
        });
    }

    // ── Scenario: Filter chips narrow the product table ─────────────────────────

    [Fact]
    public void FilterChip_LowStock_NarrowsTableToLowStockOnly()
    {
        using var ctx = new TestContext();
        RegisterModalDependencies(ctx);
        ctx.Services.AddScoped(_ => CreateHandlerMock().Object);

        var cut = ctx.RenderWithAuth<AdminProduct>(
            TestHelpers.Authenticated("admin1", "admin@test", "Admin"));

        // Initially all three products render (identified by distinct product names)
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Alpha Ring", cut.Markup);     // In Stock
            Assert.Contains("Beta Necklace", cut.Markup);  // Low Stock
            Assert.Contains("Gamma Bracelet", cut.Markup); // Out of Stock
        });

        // Click the "Low Stock" filter chip
        cut.FindAll("button").First(b => b.TextContent.Trim() == "Low Stock").Click();

        // Only the Low Stock product remains; handler is mocked to filter server-side
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Beta Necklace", cut.Markup);
            Assert.DoesNotContain("Alpha Ring", cut.Markup);
            Assert.DoesNotContain("Gamma Bracelet", cut.Markup);
        });
    }
}
