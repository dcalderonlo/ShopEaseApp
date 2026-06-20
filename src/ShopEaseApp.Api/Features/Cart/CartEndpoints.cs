using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ShopEaseApp.Api.Infrastructure.Endpoints;

namespace ShopEaseApp.Api.Features.Cart;

public class CartEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/cart")
            .WithTags("Cart")
            .RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, CartService service) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var cart = await service.GetCartAsync(userId);
            return Results.Ok(cart);
        }).WithName("GetCart");

        group.MapPost("/items", async (
            [FromBody] AddItemRequest request,
            ClaimsPrincipal user,
            CartService service,
            IValidator<AddItemRequest> validator) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var (success, cart, error) = await service.AddItemAsync(userId, request);
            return success ? Results.Ok(cart) : Results.BadRequest(new { error });
        }).WithName("AddCartItem");

        group.MapPut("/items", async (
            [FromBody] UpdateItemRequest request,
            ClaimsPrincipal user,
            CartService service,
            IValidator<UpdateItemRequest> validator) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var (success, cart, error) = await service.UpdateItemAsync(userId, request);
            return success ? Results.Ok(cart) : Results.BadRequest(new { error });
        }).WithName("UpdateCartItem");

        group.MapDelete("/items/{variantId:int}", async (
            int variantId,
            ClaimsPrincipal user,
            CartService service) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var cart = await service.RemoveItemAsync(userId, variantId);
            return Results.Ok(cart);
        }).WithName("RemoveCartItem");

        group.MapDelete("/", async (ClaimsPrincipal user, CartService service) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await service.ClearAsync(userId);
            return Results.NoContent();
        }).WithName("ClearCart");
    }
}
