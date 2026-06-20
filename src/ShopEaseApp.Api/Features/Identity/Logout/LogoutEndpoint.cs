using ShopEaseApp.Api.Infrastructure.Endpoints;

namespace ShopEaseApp.Api.Features.Identity.Logout;

public class LogoutEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        app.MapPost("/api/auth/logout", (HttpContext httpContext) =>
        {
            httpContext.Response.Cookies.Delete("auth_token");
            return Results.Ok(new { message = "Logged out successfully." });
        })
        .WithName("LogoutUser")
        .WithTags("Identity")
        .RequireAuthorization();
    }
}
