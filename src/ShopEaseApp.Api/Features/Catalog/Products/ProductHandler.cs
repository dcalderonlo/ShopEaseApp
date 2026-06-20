using Microsoft.EntityFrameworkCore;
using ShopEaseApp.Api.Domain;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Api.Features.Catalog.Products;

public class ProductHandler
{
    private readonly AppDbContext _db;

    public ProductHandler(AppDbContext db) => _db = db;

    private static ProductResponse ToResponse(Product p) => new(
        p.Id, p.Name, p.Description,
        p.CategoryId, p.Category.Name,
        p.ImageUrls,
        p.Variants.Select(v => new VariantSummary(v.Id, v.Name, v.Price, v.Stock)));

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<IEnumerable<ProductResponse>> GetAllAsync() =>
        await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Select(p => ToResponse(p))
            .ToListAsync();

    public async Task<ProductResponse?> GetByIdAsync(int id) =>
        await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Where(p => p.Id == id)
            .Select(p => ToResponse(p))
            .FirstOrDefaultAsync();

    // ── Commands ──────────────────────────────────────────────────────────────

    public async Task<(bool Success, ProductResponse? Response, string? Error)> CreateAsync(
        CreateProductRequest request)
    {
        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists) return (false, null, "Category not found.");

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            ImageUrls = request.ImageUrls.ToList(),
            Variants = request.Variants.Select(v => new ProductVariant
            {
                Name = v.Name,
                Price = v.Price,
                Stock = v.Stock
            }).ToList()
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        // Reload with navigation properties
        await _db.Entry(product).Reference(p => p.Category).LoadAsync();
        return (true, ToResponse(product), null);
    }

    public async Task<(bool Success, ProductResponse? Response, string? Error)> UpdateAsync(
        int id, UpdateProductRequest request)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null) return (false, null, "Product not found.");

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists) return (false, null, "Category not found.");

        product.Name = request.Name;
        product.Description = request.Description;
        product.CategoryId = request.CategoryId;
        product.ImageUrls = request.ImageUrls.ToList();

        await _db.SaveChangesAsync();
        await _db.Entry(product).Reference(p => p.Category).LoadAsync();

        return (true, ToResponse(product), null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return false;

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return true;
    }
}
