namespace ShopEaseApp.Api.Features.Catalog.Products;

public record VariantSummary(int Id, string Name, decimal Price, int Stock, int MinimumStockLevel, string Status);

public record ProductResponse(
    int Id,
    string Name,
    string? Description,
    int CategoryId,
    string CategoryName,
    IEnumerable<string> ImageUrls,
    IEnumerable<VariantSummary> Variants);

public record CreateVariantRequest(string Name, decimal Price, int Stock, int MinimumStockLevel = 5);

public record CreateProductRequest(
    string Name,
    string? Description,
    int CategoryId,
    IEnumerable<string> ImageUrls,
    IEnumerable<CreateVariantRequest> Variants);

public record UpdateProductRequest(
    string Name,
    string? Description,
    int CategoryId,
    IEnumerable<string> ImageUrls);
