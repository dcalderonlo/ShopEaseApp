using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ShopEaseApp.Api.Infrastructure.Endpoints;

namespace ShopEaseApp.Api.Features.Identity.Login;

public class LoginEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        app.MapPost("/api/auth/login", async (
            [FromBody] LoginRequest request,
            LoginHandler handler,
            IValidator<LoginRequest> validator,
            HttpContext httpContext,
            IWebHostEnvironment env) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var (success, response) = await handler.HandleAsync(request);
            if (!success || response is null)
                return Results.Unauthorized();

            // Set HttpOnly cookie alongside the bearer token in the response body
            httpContext.Response.Cookies.Append("auth_token", response.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = !env.IsDevelopment(),
                SameSite = SameSiteMode.Lax,
                Expires = response.ExpiresAt
            });

            return Results.Ok(response);
        })
        .WithName("LoginUser")
        .WithTags("Identity")
        .AllowAnonymous();
    }
}
