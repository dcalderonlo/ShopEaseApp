using ShopEaseApp.Api.Infrastructure.Endpoints;

namespace ShopEaseApp.Api.Features.Admin.Dashboard;

public class AdminEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/admin").WithTags("Admin")
            .RequireAuthorization(p => p.RequireRole("Admin"));

        group.MapGet("/dashboard", async (AdminDashboardHandler handler) =>
            Results.Ok(await handler.GetMetricsAsync()))
            .WithName("GetAdminDashboard");

        group.MapGet("/products", async (string? status, AdminDashboardHandler handler) =>
            Results.Ok(await handler.GetProductsAsync(status)))
            .WithName("GetAdminProducts");
    }
}
