using System.Security.Claims;
using ShopEaseApp.Api.Infrastructure.Endpoints;

namespace ShopEaseApp.Api.Features.Orders;

public class OrderEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        // Customer — create order from cart
        group.MapPost("/", async (ClaimsPrincipal user, OrderHandler handler) =>
        {
            var customerId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var (success, order, error) = await handler.CreateFromCartAsync(customerId);
            if (!success) return Results.BadRequest(new { error });
            return Results.Created($"/api/orders/{order!.Id}", order);
        })
        .WithName("CreateOrder")
        .RequireAuthorization(p => p.RequireRole("Customer", "Admin"));

        // Customer — view own order by ID
        group.MapGet("/{id:int}", async (int id, ClaimsPrincipal user, OrderHandler handler) =>
        {
            var customerId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var order = await handler.GetByIdAsync(id, customerId);
            return order is null ? Results.NotFound() : Results.Ok(order);
        })
        .WithName("GetOrder");

        // Customer — own order history
        group.MapGet("/my", async (ClaimsPrincipal user, OrderHandler handler) =>
        {
            var customerId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            return Results.Ok(await handler.GetCustomerHistoryAsync(customerId));
        })
        .WithName("GetMyOrders");

        // Admin — all orders
        group.MapGet("/", async (OrderHandler handler) =>
            Results.Ok(await handler.GetAllAsync()))
        .WithName("GetAllOrders")
        .RequireAuthorization(p => p.RequireRole("Admin"));
    }
}
