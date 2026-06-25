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
    string Status,
    decimal Price);

/// <summary>DTO for rendering a product row in admin tables.</summary>
public record AdminProductItem(
    int ProductId,
    int VariantId,
    string ProductName,
    string CategoryName,
    int Stock,
    string Status,
    decimal Price);

/// <summary>DTO for rendering a category row in admin tables.</summary>
public record AdminCategoryItem(
    int CategoryId,
    string CategoryName,
    string? CategoryDescription,
    int ProductCount);
