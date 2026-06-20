using Microsoft.EntityFrameworkCore;
using ShopEaseApp.Api.Domain;
using ShopEaseApp.Api.Features.Catalog.Products;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Tests.Features.Catalog;

public class ProductHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("ProductTests_" + Guid.NewGuid())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<int> SeedCategoryAsync(AppDbContext db, string name = "Accessories")
    {
        var cat = new Category { Name = name };
        db.Categories.Add(cat);
        await db.SaveChangesAsync();
        return cat.Id;
    }

    // ── Scenario: Browse available products ───────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsProductsWithCategoryAndVariants()
    {
        await using var db = CreateDb();
        var catId = await SeedCategoryAsync(db);
        db.Products.Add(new Product
        {
            Name = "Gold Earring",
            CategoryId = catId,
            ImageUrls = new List<string> { "http://img.test/earring.jpg" },
            Variants = new List<ProductVariant>
            {
                new() { Name = "Gold", Price = 25.99m, Stock = 10 }
            }
        });
        await db.SaveChangesAsync();

        var handler = new ProductHandler(db);
        var results = await handler.GetAllAsync();

        Assert.Single(results);
        var product = results.First();
        Assert.Equal("Gold Earring", product.Name);
        Assert.Single(product.Variants);
        Assert.Equal(25.99m, product.Variants.First().Price);
    }

    // ── Scenario: Browse with no matches ──────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyListWhenNoProducts()
    {
        await using var db = CreateDb();
        var handler = new ProductHandler(db);
        var results = await handler.GetAllAsync();
        Assert.Empty(results);
    }

    // ── Scenario: Admin creates a product with at least one variant ───────────

    [Fact]
    public async Task CreateAsync_PersistsProductWithVariantsAndReturnsResponse()
    {
        await using var db = CreateDb();
        var catId = await SeedCategoryAsync(db);
        var handler = new ProductHandler(db);

        var request = new CreateProductRequest(
            "Silver Necklace", "Sterling silver chain",
            catId,
            new[] { "http://img/necklace.jpg" },
            new[] { new CreateVariantRequest("45cm", 49.99m, 5) });

        var (success, response, error) = await handler.CreateAsync(request);

        Assert.True(success);
        Assert.NotNull(response);
        Assert.Equal("Silver Necklace", response!.Name);
        Assert.Single(response.Variants);
        Assert.Equal(49.99m, response.Variants.First().Price);
    }

    // ── Scenario: Product does not exist ──────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsNullForUnknownProduct()
    {
        await using var db = CreateDb();
        var handler = new ProductHandler(db);
        var result = await handler.GetByIdAsync(999);
        Assert.Null(result);
    }

    // ── Scenario: Update targets unknown product ──────────────────────────────

    [Fact]
    public async Task UpdateAsync_ReturnsFalseForUnknownProduct()
    {
        await using var db = CreateDb();
        var catId = await SeedCategoryAsync(db);
        var handler = new ProductHandler(db);

        var (success, _, error) = await handler.UpdateAsync(999,
            new UpdateProductRequest("X", null, catId, Array.Empty<string>()));

        Assert.False(success);
        Assert.Equal("Product not found.", error);
    }
}
