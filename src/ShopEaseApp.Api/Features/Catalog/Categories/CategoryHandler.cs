using Microsoft.EntityFrameworkCore;
using ShopEaseApp.Api.Domain;
using ShopEaseApp.Api.Features.Admin.Dashboard;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Api.Features.Catalog.Categories;

public class CategoryHandler(AppDbContext db)
{
    private readonly AppDbContext _db = db;

  // ── Queries ───────────────────────────────────────────────────────────────

  public async Task<IEnumerable<CategoryResponse>> GetAllAsync() =>
        await _db.Categories
            .AsNoTracking()
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description))
            .ToListAsync();

    public async Task<CategoryResponse?> GetByIdAsync(int id) =>
        await _db.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description))
            .FirstOrDefaultAsync();

    public async Task<List<AdminCategoryItem>> GetCategoriesAsync() =>
        await _db.Categories
            .AsNoTracking()
            .Select(c => new AdminCategoryItem(c.Id, c.Name, c.Description, c.Products.Count))
            .ToListAsync();

    // ── Commands ──────────────────────────────────────────────────────────────

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var category = new Category { Name = request.Name, Description = request.Description };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryResponse?> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return null;

        category.Name = request.Name;
        category.Description = request.Description;
        await _db.SaveChangesAsync();
        return new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var category = await _db.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null) return (false, "Category not found.");
        if (category.Products.Count != 0) return (false, "Cannot delete a category with assigned products.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return (true, null);
    }
}
