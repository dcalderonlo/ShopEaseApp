namespace ShopEaseApp.Api.Features.Catalog.Categories;

public record CategoryResponse(int Id, string Name, string? Description);

public record CreateCategoryRequest(string Name, string? Description);
public record UpdateCategoryRequest(string Name, string? Description);
