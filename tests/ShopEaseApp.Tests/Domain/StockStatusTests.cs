using ShopEaseApp.Api.Domain;

namespace ShopEaseApp.Tests.Domain;

public class StockStatusTests
{
    // ── Scenario: Status computed correctly across thresholds ──────────────────
    // Spec: In Stock when Stock > MinimumStockLevel,
    //       Low Stock when 0 < Stock <= MinimumStockLevel,
    //       Out of Stock when Stock == 0.

    [Theory]
    [InlineData(10, 5, "In Stock")]
    [InlineData(6, 5, "In Stock")]   // stock = min + 1 (just above threshold)
    [InlineData(5, 5, "Low Stock")]  // stock = min  (boundary: <= min)
    [InlineData(3, 5, "Low Stock")]
    [InlineData(1, 5, "Low Stock")]
    [InlineData(0, 5, "Out of Stock")]
    public void Compute_ReturnsExpectedStatus(int stock, int minimumStockLevel, string expected)
    {
        Assert.Equal(expected, StockStatus.Compute(stock, minimumStockLevel));
    }

    // ── Scenario: MinimumStockLevel varies per variant ─────────────────────────
    // Two variants of the same product with different MinimumStockLevel values,
    // both with Stock = 4, must reflect their own threshold.

    [Fact]
    public void Compute_RespectsPerVariantMinimumStockLevel()
    {
        Assert.Equal("In Stock", StockStatus.Compute(4, 3));
        Assert.Equal("Low Stock", StockStatus.Compute(4, 5));
    }
}
