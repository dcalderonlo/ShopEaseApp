using Microsoft.EntityFrameworkCore;
using ShopEaseApp.Api.Domain;
using ShopEaseApp.Api.Features.Catalog.Categories;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Tests.Features.Catalog;

public class CategoryHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("CatalogTests_" + Guid.NewGuid())
            .Options;
        return new AppDbContext(options);
    }

    // ── Scenario: Browse categories ───────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsCategoriesWhenExist()
    {
        await using var db = CreateDb();
        db.Categories.Add(new Category { Name = "Earrings", Description = "Fine jewelry" });
        db.Categories.Add(new Category { Name = "Handbags" });
        await db.SaveChangesAsync();

        var handler = new CategoryHandler(db);
        var result = await handler.GetAllAsync();

        Assert.Equal(2, result.Count());
        Assert.Contains(result, c => c.Name == "Earrings");
    }

    // ── Scenario: No categories exist ─────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyWhenNoneExist()
    {
        await using var db = CreateDb();
        var handler = new CategoryHandler(db);
        var result = await handler.GetAllAsync();
        Assert.Empty(result);
    }

    // ── Scenario: Admin creates a category ────────────────────────────────────

    [Fact]
    public async Task CreateAsync_PersistsCategoryAndReturnsResponse()
    {
        await using var db = CreateDb();
        var handler = new CategoryHandler(db);
        var request = new CreateCategoryRequest("Necklaces", "Gold and silver");

        var result = await handler.CreateAsync(request);

        Assert.True(result.Id > 0);
        Assert.Equal("Necklaces", result.Name);
        Assert.Equal("Gold and silver", result.Description);
        Assert.Equal(1, await db.Categories.CountAsync());
    }

    // ── Scenario: Category deletion blocked by assigned products ─────────────

    [Fact]
    public async Task DeleteAsync_RejectsWhenProductsAssigned()
    {
        await using var db = CreateDb();
        var category = new Category { Name = "Rings" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        db.Products.Add(new Product
        {
            Name = "Gold Ring",
            CategoryId = category.Id,
            ImageUrls = new List<string>()
        });
        await db.SaveChangesAsync();

        var handler = new CategoryHandler(db);
        var (success, error) = await handler.DeleteAsync(category.Id);

        Assert.False(success);
        Assert.NotNull(error);
        Assert.Equal(1, await db.Categories.CountAsync()); // still exists
    }

    // ── Triangulation: delete non-existent category ──────────────────────────

    [Fact]
    public async Task DeleteAsync_ReturnsFalseForUnknownId()
    {
        await using var db = CreateDb();
        var handler = new CategoryHandler(db);
        var (success, error) = await handler.DeleteAsync(999);
        Assert.False(success);
        Assert.Equal("Category not found.", error);
    }
}
