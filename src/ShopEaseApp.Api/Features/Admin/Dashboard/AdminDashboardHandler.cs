using Microsoft.EntityFrameworkCore;
using ShopEaseApp.Api.Domain;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Api.Features.Admin.Dashboard;

public class AdminDashboardHandler
{
    private readonly AppDbContext _db;
    public AdminDashboardHandler(AppDbContext db) => _db = db;

    public virtual async Task<DashboardResponse> GetMetricsAsync()
    {
        var totalProducts = await _db.Products.CountAsync();
        var totalSkus = await _db.ProductVariants.CountAsync();
        var lowStock = await _db.ProductVariants
            .CountAsync(v => v.Stock > 0 && v.Stock <= v.MinimumStockLevel);
        var inventoryValue = await _db.ProductVariants
            .SumAsync(v => v.Price * v.Stock);

        return new DashboardResponse(totalProducts, totalSkus, lowStock, inventoryValue);
    }

    public virtual async Task<List<AdminProductResponse>> GetProductsAsync(string? statusFilter = null)
    {
        var variants = await _db.ProductVariants
            .AsNoTracking()
            .Include(v => v.Product).ThenInclude(p => p.Category)
            .ToListAsync();

        var result = variants.Select(v => new AdminProductResponse(
            v.Product.Id, v.Product.Name,
            v.Product.Category.Name,
            v.Id, v.Name,
            v.Stock, v.MinimumStockLevel,
            StockStatus.Compute(v.Stock, v.MinimumStockLevel),
            v.Price));

        if (!string.IsNullOrEmpty(statusFilter))
            result = result.Where(r => r.Status == statusFilter);

        return result.OrderBy(r => r.ProductName).ToList();
    }
}
