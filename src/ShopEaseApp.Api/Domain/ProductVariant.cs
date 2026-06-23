namespace ShopEaseApp.Api.Domain;

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string Name { get; set; } = string.Empty;  // e.g. "Red", "Large"
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int MinimumStockLevel { get; set; } = 5;
}
