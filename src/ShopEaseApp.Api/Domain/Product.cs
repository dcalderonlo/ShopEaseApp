namespace ShopEaseApp.Api.Domain;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<string> ImageUrls { get; set; } = [];
    public ICollection<ProductVariant> Variants { get; set; } = [];
}
