using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ShopEaseApp.Api.Infrastructure.Endpoints;

namespace ShopEaseApp.Api.Features.Catalog.Products;

public class ProductEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Catalog");

        // Public endpoints
        group.MapGet("/", async (ProductHandler handler) =>
            Results.Ok(await handler.GetAllAsync()))
            .WithName("GetProducts")
            .AllowAnonymous()
            .CacheOutput(p => p.Expire(TimeSpan.FromMinutes(5)).Tag("products"));

        group.MapGet("/{id:int}", async (int id, ProductHandler handler) =>
        {
            var product = await handler.GetByIdAsync(id);
            return product is null ? Results.NotFound() : Results.Ok(product);
        })
        .WithName("GetProductById")
        .AllowAnonymous();

        // Admin-only endpoints
        group.MapPost("/", async (
            [FromBody] CreateProductRequest request,
            ProductHandler handler,
            IValidator<CreateProductRequest> validator) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

            var (success, response, error) = await handler.CreateAsync(request);
            if (!success) return Results.BadRequest(new { error });
            return Results.Created($"/api/products/{response!.Id}", response);
        })
        .WithName("CreateProduct")
        .RequireAuthorization(p => p.RequireRole("Admin"));

        group.MapPut("/{id:int}", async (
            int id,
            [FromBody] UpdateProductRequest request,
            ProductHandler handler,
            IValidator<UpdateProductRequest> validator) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

            var (success, response, error) = await handler.UpdateAsync(id, request);
            if (!success)
                return error == "Product not found." ? Results.NotFound() : Results.BadRequest(new { error });
            return Results.Ok(response);
        })
        .WithName("UpdateProduct")
        .RequireAuthorization(p => p.RequireRole("Admin"));

        group.MapDelete("/{id:int}", async (int id, ProductHandler handler) =>
        {
            var deleted = await handler.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteProduct")
        .RequireAuthorization(p => p.RequireRole("Admin"));
    }
}
