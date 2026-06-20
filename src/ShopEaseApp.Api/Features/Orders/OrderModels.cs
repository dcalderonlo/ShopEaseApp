namespace ShopEaseApp.Api.Features.Orders;

public record OrderItemResponse(
    int VariantId,
    string VariantName,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record OrderResponse(
    int Id,
    string CustomerId,
    string Status,
    decimal Total,
    DateTime CreatedAt,
    IEnumerable<OrderItemResponse> Items);
