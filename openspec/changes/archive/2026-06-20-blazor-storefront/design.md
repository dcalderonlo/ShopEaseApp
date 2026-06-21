# Design: Blazor Server Storefront UI

## Technical Approach

Integrate Blazor Server directly into `ShopEaseApp.Api`, sharing the existing DI container, handlers, and cookie auth. Components are organized by vertical slice under `Features/{Feature}/Components/`, mirroring the current architecture. No HTTP calls — components inject handlers directly.

## Architecture Decisions

| Decision | Choice | Alternative | Rationale |
|----------|--------|-------------|-----------|
| Integration pattern | Same project (`ShopEaseApp.Api`) | Separate `ShopEaseApp.Web` project | Direct handler injection, native cookie auth, single deployable. Avoids service duplication and HTTP overhead. |
| Auth flow | Custom `ServerAuthenticationStateProvider` reading `HttpContext.User` | JS token management or separate auth | The `auth_token` cookie already authenticates. Blazor circuits run server-side, so `HttpContext` is available at render time. No JS interop needed. |
| Component organization | Feature-co-located (`Features/{Name}/Components/`) | Flat `Components/` folder | Mirrors existing vertical slice structure. Keeps related UI with its handler/models. |
| Component→Handler interaction | Direct `@inject` of handlers | HTTP client calls | Blazor Server runs in-process. Injecting `ProductHandler`, `CartService`, etc. avoids serialization and model duplication. Rule: never inject `AppDbContext` directly. |
| Testing | bUnit for components; existing `WebApplicationFactory` for API | Selenium or Playwright | bUnit is the standard for Blazor component testing with DI. Existing API tests remain unchanged. |

## Data Flow

```
Browser ──SignalR──→ Blazor Hub (server circuit)
                         │
                    Component @inject
                         │
              ┌──────────┼──────────┐
              ▼          ▼          ▼
         ProductHandler CartService OrderHandler
              │          │          │
              └──────────┼──────────┘
                         ▼
                    AppDbContext (scoped)
```

Auth flow: `HttpContext.User` → `ServerAuthenticationStateProvider` → `<AuthorizeView>` / `<CascadingAuthenticationState>` → conditional UI rendering.

Login sets `auth_token` cookie via existing `LoginHandler`. After login, call `NotifyAuthenticationStateChanged` to update the Blazor circuit without full page reload.

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `src/ShopEaseApp.Api/ShopEaseApp.Api.csproj` | Modify | Add `Microsoft.AspNetCore.Components.Server` package |
| `src/ShopEaseApp.Api/Program.cs` | Modify | Add `AddServerSideBlazor()`, `AddRazorComponents()`, `MapBlazorHub()`, `MapRazorComponents<App>()`, register `AuthenticationStateProvider` |
| `src/ShopEaseApp.Api/Components/App.razor` | Create | Root Blazor component with `<Router>`, `<CascadingAuthenticationState>` |
| `src/ShopEaseApp.Api/Components/Routes.razor` | Create | Route wrapper with layout |
| `src/ShopEaseApp.Api/Components/_Imports.razor` | Create | Global usings for all components |
| `src/ShopEaseApp.Api/Components/Layout/MainLayout.razor` | Create | Page layout with NavMenu and body |
| `src/ShopEaseApp.Api/Components/Layout/NavMenu.razor` | Create | Navbar with auth-aware links + cart badge |
| `src/ShopEaseApp.Api/Components/Pages/Home.razor` | Create | Route `/` → catalog |
| `src/ShopEaseApp.Api/Components/Pages/ProductDetail.razor` | Create | Route `/product/{id}` |
| `src/ShopEaseApp.Api/Components/Pages/Cart.razor` | Create | Route `/cart` (auth required) |
| `src/ShopEaseApp.Api/Components/Pages/Checkout.razor` | Create | Route `/checkout` (auth required) |
| `src/ShopEaseApp.Api/Components/Pages/Orders.razor` | Create | Route `/orders` (auth required) |
| `src/ShopEaseApp.Api/Components/Pages/Login.razor` | Create | Route `/login` |
| `src/ShopEaseApp.Api/Components/Pages/Register.razor` | Create | Route `/register` |
| `src/ShopEaseApp.Api/Infrastructure/Auth/ServerAuthenticationStateProvider.cs` | Create | Reads `HttpContext.User`, supports `NotifyAuthenticationStateChanged` |
| `tests/ShopEaseApp.Blazor.Tests/` | Create | bUnit + xUnit test project for component tests |

## Interfaces / Contracts

```csharp
// ServerAuthenticationStateProvider
public class ServerAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _http;
    public ServerAuthenticationStateProvider(IHttpContextAccessor http) => _http = http;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = _http.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        return Task.FromResult(new AuthenticationState(user));
    }

    public void NotifyAuthenticationChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
```

Program.cs additions (after existing handler registrations):
```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
```

Middleware additions (after `UseEndpointDefinitions()`):
```csharp
app.MapStaticAssets();
app.MapRazorComponents<ShopEaseApp.Api.Components.App>()
   .AddInteractiveServerRenderMode();
```

## Testing Strategy

| Layer | What | Approach |
|-------|------|----------|
| Unit (bUnit) | Component rendering, auth-gated UI, form validation | Render in test host with mocked handlers |
| Integration | Existing API tests via `WebApplicationFactory` | Unchanged — API endpoints still work. Use `ASPNETCORE_ENVIRONMENT=Testing` to skip Blazor rendering. |
| Component | Login flow, cart add/remove, checkout happy path | bUnit + DI with InMemory DbContext and fake `CartService` |

## Migration / Rollout

No migration required. Blazor Server is additive — existing API endpoints and tests are unaffected. The Scalar docs at `/scalar/v1` continue to work in Development.

## Open Questions

- [ ] None
