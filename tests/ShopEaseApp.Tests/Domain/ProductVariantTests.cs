using ShopEaseApp.Api.Domain;

namespace ShopEaseApp.Tests.Domain;

public class ProductVariantTests
{
    // ── Scenario: MinimumStockLevel defaults to 5 on a new variant ────────────

    [Fact]
    public void MinimumStockLevel_DefaultsToFive()
    {
        var variant = new ProductVariant();
        Assert.Equal(5, variant.MinimumStockLevel);
    }

    // ── Scenario: MinimumStockLevel is settable per variant ────────────────────

    [Fact]
    public void MinimumStockLevel_CanBeSet()
    {
        var variant = new ProductVariant { MinimumStockLevel = 12 };
        Assert.Equal(12, variant.MinimumStockLevel);
    }
}
