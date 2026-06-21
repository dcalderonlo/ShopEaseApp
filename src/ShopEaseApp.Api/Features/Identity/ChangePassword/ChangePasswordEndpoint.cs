using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopEaseApp.Api.Infrastructure.Data;
using ShopEaseApp.Api.Infrastructure.Endpoints;

namespace ShopEaseApp.Api.Features.Identity.ChangePassword;

/// <summary>
/// POST /api/auth/change-password — authenticated endpoint that changes the
/// current user's password. Resolves the user from the bearer-token claims,
/// validates the payload, then delegates to ChangePasswordHandler.
/// </summary>
public class ChangePasswordEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        app.MapPost("/api/auth/change-password", async (
            [FromBody] ChangePasswordRequest request,
            ClaimsPrincipal user,
            ChangePasswordHandler handler,
            UserManager<AppUser> userManager,
            IValidator<ChangePasswordRequest> validator) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var appUser = await userManager.FindByIdAsync(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (appUser is null)
                return Results.Unauthorized();

            var (success, error) = await handler.HandleAsync(appUser, request);
            if (!success)
                return Results.BadRequest(new { error });

            return Results.Ok(new { message = "Password changed successfully." });
        })
        .WithName("ChangePassword")
        .WithTags("Identity")
        .RequireAuthorization();
    }
}
