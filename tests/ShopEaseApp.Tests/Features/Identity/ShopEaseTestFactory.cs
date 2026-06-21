using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Tests.Features.Identity;

/// <summary>
/// Custom WebApplicationFactory that replaces SQL Server with InMemory
/// and ensures the Identity schema is created before tests execute.
/// </summary>
public class ShopEaseTestFactory : WebApplicationFactory<Program>
{
    // Each test class instance gets its own isolated InMemory database
    private readonly string _dbName = "ShopEaseTests_" + Guid.NewGuid();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set Testing environment so RoleSeeder is skipped in Program.cs
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestSecretKeyForIntegrationTests_32CharsMin!",
                ["Jwt:Issuer"] = "ShopEaseTest",
                ["Jwt:Audience"] = "ShopEaseTest",
                ["Jwt:ExpiryMinutes"] = "60",
                ["ConnectionStrings:Redis"] = "localhost:6379"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove ALL EF Core DbContext-related service registrations
            // to prevent the internal EF provider cache conflict
            var efDescriptors = services
                .Where(d =>
                    d.ServiceType.FullName != null &&
                    (d.ServiceType.FullName.Contains("EntityFrameworkCore") ||
                     d.ServiceType == typeof(AppDbContext) ||
                     d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                     (d.ServiceType.IsGenericType &&
                      d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))))
                .ToList();

            foreach (var d in efDescriptors) services.Remove(d);

            // Add a fresh InMemory DbContext
            // NOTE: Do NOT call EnableServiceProviderCaching(false) — it isolates DbContext
            // instances and breaks shared InMemory state across scopes.
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Override the JWT bearer signing key/issuer/audience with the test values.
            // The middleware captures the signing key EAGERLY at startup from
            // builder.Configuration, which (under WebApplicationFactory) still holds the
            // appsettings.json key at that instant — the in-memory test config added via
            // ConfigureAppConfiguration above is applied later. Re-configuring the
            // "Bearer"-named JwtBearerOptions here (AddJwtBearer registers its setup
            // under the scheme name "Bearer", so the override must use the same name to
            // take effect) ensures validation uses the SAME test key that JwtService
            // signs with, so authenticated integration tests actually authenticate.
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TestSecretKeyForIntegrationTests_32CharsMin!"))
                    { KeyId = "ShopEase-default-key" };
                options.TokenValidationParameters.ValidIssuer = "ShopEaseTest";
                options.TokenValidationParameters.ValidAudience = "ShopEaseTest";
                options.TokenValidationParameters.TryAllIssuerSigningKeys = true;
            });
        });
    }

    /// <summary>
    /// Creates an HTTP client. Roles are seeded by Program.cs at startup (idempotent).
    /// </summary>
    public HttpClient CreateAuthClient()
    {
        var client = CreateClient();

        // Ensure InMemory schema exists (EnsureCreated is idempotent)
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        return client;
    }
}
