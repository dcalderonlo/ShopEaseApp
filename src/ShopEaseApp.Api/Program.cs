using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Components.Authorization;
using Scalar.AspNetCore;
using Serilog;
using ShopEaseApp.Api.Components;
using ShopEaseApp.Api.Infrastructure.Data;
using ShopEaseApp.Api.Infrastructure.Data.Seeding;
using ShopEaseApp.Api.Infrastructure.Endpoints;

// ── Serilog bootstrap (reads from appsettings) ──────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// ── Database ─────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Identity ─────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ── JWT + Cookie dual auth ────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        // Read JWT from HttpOnly cookie first, then fall back to Authorization header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var cookieToken = context.Request.Cookies["auth_token"];
                if (!string.IsNullOrEmpty(cookieToken))
                    context.Token = cookieToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── Caching ───────────────────────────────────────────────────────────────────
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromMinutes(5)));
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "ShopEase:";
});

builder.Services.AddMemoryCache();

// ── Resilience (Polly) ────────────────────────────────────────────────────────
builder.Services.AddHttpClient("resilient")
    .AddStandardResilienceHandler();

// ── Scalar / OpenAPI ──────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// ── FluentValidation ─────────────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ── Feature handlers / services ───────────────────────────────────────────────
builder.Services.AddScoped<ShopEaseApp.Api.Infrastructure.Auth.JwtService>();
builder.Services.AddScoped<ShopEaseApp.Api.Features.Identity.Register.RegisterHandler>();
builder.Services.AddScoped<ShopEaseApp.Api.Features.Identity.Login.LoginHandler>();
builder.Services.AddScoped<ShopEaseApp.Api.Features.Catalog.Categories.CategoryHandler>();
builder.Services.AddScoped<ShopEaseApp.Api.Features.Catalog.Products.ProductHandler>();
builder.Services.AddScoped<ShopEaseApp.Api.Features.Cart.CartService>();
builder.Services.AddScoped<ShopEaseApp.Api.Features.Orders.OrderHandler>();

// ── Blazor Server storefront ─────────────────────────────────────────────────
// Microsoft.AspNetCore.Components.Server is provided by the .NET 10 shared
// framework (Microsoft.AspNetCore.App) via the Web SDK — no separate NuGet
// package is required. AddInteractiveServerComponents() enables the SignalR
// circuit + interactive render mode used by the storefront components.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── Endpoint definitions (Vertical Slice auto-registration) ──────────────────
builder.Services.AddEndpointDefinitions(typeof(Program));

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Scalar / OpenAPI (Development only) ──────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "ShopEase API";
        options.Theme = ScalarTheme.Moon;
    });
}

// ── Seed roles (always run — idempotent; roles only created if missing) ──────
try
{
    await RoleSeeder.SeedRolesAsync(app.Services);
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Role seeding skipped — database may not be available yet.");
}

// ── Seed default admin user ────────────────────────────────────────────────────
try
{
    await AdminSeeder.SeedAdminAsync(app.Services);
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Admin seeding skipped — database may not be available yet.");
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseOutputCache();

// ── Register all feature endpoints ───────────────────────────────────────────
app.UseEndpointDefinitions();

// ── Blazor Server storefront (registered AFTER API endpoints so /api/* wins) ─
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Expose Program for WebApplicationFactory in tests
public partial class Program { }
