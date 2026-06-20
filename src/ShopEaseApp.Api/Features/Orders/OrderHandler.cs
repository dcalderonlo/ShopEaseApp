using Microsoft.EntityFrameworkCore;
using ShopEaseApp.Api.Domain;
using ShopEaseApp.Api.Features.Cart;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Api.Features.Orders;

public class OrderHandler(AppDbContext db, CartService cart)
{
    private readonly AppDbContext _db = db;
    private readonly CartService _cart = cart;

  // ── Map helper ────────────────────────────────────────────────────────────

  private static OrderResponse ToResponse(Order o) => new(
        o.Id, o.CustomerId, o.Status.ToString(), o.Total, o.CreatedAt,
        o.Items.Select(i => new OrderItemResponse(
            i.VariantId, i.VariantName, i.ProductName,
            i.Quantity, i.UnitPrice, i.Quantity * i.UnitPrice)));

    // ── Scenario: Customer checks out successfully ────────────────────────────

    public async Task<(bool Success, OrderResponse? Order, string? Error)> CreateFromCartAsync(
        string customerId)
    {
        var cartResponse = await _cart.GetCartAsync(customerId);
        var cartItems = cartResponse.Items.ToList();

        if (cartItems.Count == 0)
            return (false, null, "Cart is empty.");

        // Load all variants in one query and verify stock
        var variantIds = cartItems.Select(i => i.VariantId).ToList();
        var variants = await _db.ProductVariants
            .Include(v => v.Product)
            .Where(v => variantIds.Contains(v.Id))
            .ToListAsync();

        // Validate stock for every item — reject entire order if ANY item fails
        foreach (var item in cartItems)
        {
            var variant = variants.FirstOrDefault(v => v.Id == item.VariantId);
            if (variant is null)
                return (false, null, $"Variant {item.VariantId} no longer exists.");
            if (variant.Stock < item.Quantity)
                return (false, null,
                    $"Insufficient stock for '{variant.Product.Name} – {variant.Name}'. " +
                    $"Available: {variant.Stock}, requested: {item.Quantity}.");
        }

        // All stock checks passed — decrement stock and create order atomically
        // Note: In EF Core, a single SaveChangesAsync call is atomic across all
        // tracked changes. For SQL Server, a transaction would wrap this in production.
        // InMemory provider uses its own internal atomicity guarantee.
        foreach (var item in cartItems)
        {
            var variant = variants.First(v => v.Id == item.VariantId);
            variant.Stock -= item.Quantity;
        }

        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Confirmed,
            CreatedAt = DateTime.UtcNow,
            Total = cartItems.Sum(i => i.PriceSnapshot * i.Quantity),
            Items = [.. cartItems.Select(i =>
            {
                var variant = variants.First(v => v.Id == i.VariantId);
                return new OrderItem
                {
                    VariantId = i.VariantId,
                    VariantName = i.VariantName,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.PriceSnapshot
                };
            })]
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // Clear cart only after successful save
        await _cart.ClearAsync(customerId);
        return (true, ToResponse(order), null);
    }

    // ── Scenario: Customer views owned order summary ──────────────────────────

    public async Task<OrderResponse?> GetByIdAsync(int orderId, string customerId)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null) return null;

        // Customer can only view their own orders
        return order.CustomerId != customerId ? null : ToResponse(order);
    }

    // ── Scenario: Customer views their own history ────────────────────────────

    public async Task<IEnumerable<OrderResponse>> GetCustomerHistoryAsync(string customerId) =>
        await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => ToResponse(o))
            .ToListAsync();

    // ── Scenario: Admin views all orders ─────────────────────────────────────

    public async Task<IEnumerable<OrderResponse>> GetAllAsync() =>
        await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => ToResponse(o))
            .ToListAsync();
}
