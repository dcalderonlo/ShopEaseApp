using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ShopEaseApp.Api.Infrastructure.Endpoints;

namespace ShopEaseApp.Api.Features.Identity.Register;

public class RegisterEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        app.MapPost("/api/auth/register", async (
            [FromBody] RegisterRequest request,
            RegisterHandler handler,
            IValidator<RegisterRequest> validator) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var (success, response, errors) = await handler.HandleAsync(request);

            return success
                ? Results.Created($"/api/users/{response!.UserId}", response)
                : Results.BadRequest(new { errors });
        })
        .WithName("RegisterUser")
        .WithTags("Identity")
        .AllowAnonymous();
    }
}
