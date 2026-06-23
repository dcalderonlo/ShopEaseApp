using Microsoft.EntityFrameworkCore;
using ShopEaseApp.Api.Domain;
using ShopEaseApp.Api.Infrastructure.Data.Configurations;

namespace ShopEaseApp.Tests.Infrastructure.Data.Configurations;

public class ProductVariantConfigurationTests
{
    // ── Scenario: MinimumStockLevel is configured with a default value of 5 ────

    [Fact]
    public void MinimumStockLevel_HasDefaultValueFive()
    {
        var builder = new ModelBuilder();
        builder.ApplyConfiguration(new ProductVariantConfiguration());

        var property = builder.Entity<ProductVariant>()
            .Metadata.FindProperty(nameof(ProductVariant.MinimumStockLevel));

        Assert.NotNull(property);
        Assert.Equal(5, Convert.ToInt32(property!.GetDefaultValue()));
    }

    // ── Scenario: Existing configured properties remain intact (regression) ────

    [Fact]
    public void Configuration_StillMapsNamePriceAndStock()
    {
        var builder = new ModelBuilder();
        builder.ApplyConfiguration(new ProductVariantConfiguration());

        var entityType = builder.Entity<ProductVariant>().Metadata;

        Assert.NotNull(entityType.FindProperty(nameof(ProductVariant.Name)));
        Assert.NotNull(entityType.FindProperty(nameof(ProductVariant.Price)));
        Assert.NotNull(entityType.FindProperty(nameof(ProductVariant.Stock)));
    }
}
