using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ShopEaseApp.Api.Infrastructure.Endpoints;

namespace ShopEaseApp.Api.Features.Catalog.Categories;

public class CategoryEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Catalog");

        // Public endpoints
        group.MapGet("/", async (CategoryHandler handler) =>
            Results.Ok(await handler.GetAllAsync()))
            .WithName("GetCategories")
            .AllowAnonymous()
            .CacheOutput(p => p.Expire(TimeSpan.FromMinutes(10)).Tag("categories"));

        group.MapGet("/{id:int}", async (int id, CategoryHandler handler) =>
        {
            var category = await handler.GetByIdAsync(id);
            return category is null ? Results.NotFound() : Results.Ok(category);
        })
        .WithName("GetCategoryById")
        .AllowAnonymous();

        // Admin-only endpoints
        group.MapPost("/", async (
            [FromBody] CreateCategoryRequest request,
            CategoryHandler handler,
            IValidator<CreateCategoryRequest> validator) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

            var result = await handler.CreateAsync(request);
            return Results.Created($"/api/categories/{result.Id}", result);
        })
        .WithName("CreateCategory")
        .RequireAuthorization(p => p.RequireRole("Admin"));

        group.MapPut("/{id:int}", async (
            int id,
            [FromBody] UpdateCategoryRequest request,
            CategoryHandler handler,
            IValidator<UpdateCategoryRequest> validator) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

            var result = await handler.UpdateAsync(id, request);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("UpdateCategory")
        .RequireAuthorization(p => p.RequireRole("Admin"));

        group.MapDelete("/{id:int}", async (int id, CategoryHandler handler) =>
        {
            var (success, error) = await handler.DeleteAsync(id);
            if (!success)
                return error == "Category not found." ? Results.NotFound() : Results.Conflict(new { error });
            return Results.NoContent();
        })
        .WithName("DeleteCategory")
        .RequireAuthorization(p => p.RequireRole("Admin"));
    }
}
