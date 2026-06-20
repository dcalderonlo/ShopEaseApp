namespace ShopEaseApp.Api.Features.Cart;

/// <summary>
/// A single line item stored in the user's Redis cart.
/// Price is captured as a snapshot at add-time — never updated on subsequent catalog changes.
/// </summary>
public class CartItem
{
    public int VariantId { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PriceSnapshot { get; set; }
}

/// <summary>
/// Full cart returned to the client.
/// </summary>
public record CartResponse(string UserId, IEnumerable<CartItem> Items, decimal Total);

public record AddItemRequest(int VariantId, int Quantity);
public record UpdateItemRequest(int VariantId, int Quantity);
