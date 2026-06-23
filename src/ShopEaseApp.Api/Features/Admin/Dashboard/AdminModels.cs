namespace ShopEaseApp.Api.Features.Admin.Dashboard;

public record DashboardResponse(
    int TotalProducts,
    int TotalSkus,
    int LowStockItems,
    decimal InventoryValue);

public record AdminProductResponse(
    int ProductId,
    string ProductName,
    string CategoryName,
    int VariantId,
    string VariantName,
    int Stock,
    int MinimumStockLevel,
    string Status,       // "In Stock" | "Low Stock" | "Out of Stock"
    decimal Price);
