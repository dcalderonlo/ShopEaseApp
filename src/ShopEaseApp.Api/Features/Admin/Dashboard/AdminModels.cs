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
    decimal Price,
    string? ThumbnailUrl);

/// <summary>DTO for rendering a product row in admin tables.</summary>
public record AdminProductItem(
    int ProductId,
    int VariantId,
    string ProductName,
    string CategoryName,
    int Stock,
    string Status,
    decimal Price,
    string? ThumbnailUrl);

/// <summary>DTO for rendering a category row in admin tables.</summary>
public record AdminCategoryItem(
    int Id,
    string Name,
    string? Description,
    int ProductCount);

/// <summary>Form data emitted by the product create/edit modal.</summary>
public record CreateProductFormData(
    string Name,
    string? Description,
    int CategoryId,
    IEnumerable<string> ImageUrls,
    decimal Price,
    int Stock);

/// <summary>Form data emitted by the product edit modal.</summary>
public record UpdateProductFormData(
    string Name,
    string? Description,
    int CategoryId,
    IEnumerable<string> ImageUrls);
