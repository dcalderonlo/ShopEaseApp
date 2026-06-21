# Proposal: Blazor Server Storefront UI

## Intent

Add a Blazor Server storefront UI directly into the existing API project so users can browse products, manage carts, check out, and authenticate without external clients. This leverages the shared DI container and cookie auth already in place.

## Scope

### In Scope
- Blazor Server integration in `ShopEaseApp.Api`
- Public pages: catalog browse, product detail, cart, checkout, order history
- Identity pages: login, register
- Custom `AuthenticationStateProvider` for cookie auth flow
- Feature-organized components under `Features/{Name}/Components/`

### Out of Scope
- Admin UI (deferred)
- CSS framework polish (minimal styling only)
- Separate Blazor project or HTTP client calls

## Capabilities

### New Capabilities
- `blazor-storefront`: Blazor Server UI with public catalog browsing, product detail, cart view, checkout, login/register forms, and order history. Uses shared DI (handlers injected directly). Custom `AuthenticationStateProvider` for cookie auth.

### Modified Capabilities
- `infrastructure`: `Program.cs` modified to add Blazor Server services (`AddServerSideBlazor`, `MapBlazorHub`, `MapRazorComponents`), Blazor component folder structure.

## Approach

Integrate Blazor Server into `ShopEaseApp.Api`. Add the Blazor SDK, register server-side services in `Program.cs`, and organize components by vertical slice to mirror existing architecture. Components inject handlers directly — no HTTP serialization. A custom `AuthenticationStateProvider` reads `HttpContext.User` from the incoming request and propagates it into the Blazor circuit. Use `[CascadingAuthenticationState]` and `<AuthorizeView>` for role-based UI rendering.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `Program.cs` | Modified | Add `AddServerSideBlazor`, `MapBlazorHub`, `MapRazorComponents` |
| `ShopEaseApp.Api.csproj` | Modified | Add Blazor SDK package references |
| `Components/` | New | `App.razor`, `Routes.razor`, `MainLayout`, `NavMenu`, `CartSummary` |
| `Features/{Catalog,Cart,Orders,Identity}/Components/` | New | Feature-specific components |
| `Pages/` | New | Routeable Blazor pages |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Circuit-scoped service lifetime | Med | Keep handlers stateless; no long-lived scoped caches |
| Prerendering auth gap | Med | Graceful loading state when `HttpContext` is unavailable |
| Test bootstrap complexity | Low | Document `WebApplicationFactory` Blazor bootstrap pattern |

## Rollback Plan

Remove `AddServerSideBlazor`/`MapBlazorHub`/`MapRazorComponents` from `Program.cs`, delete `Components/` and `Pages/` folders, and revert `csproj` changes.

## Dependencies

None.

## Success Criteria

- [ ] `dotnet build` passes with Blazor Server added
- [ ] Guest can browse products and categories via UI
- [ ] Register → Login flow works through Blazor pages
- [ ] Auth cookie persists across navigation
- [ ] Add to cart → checkout → order confirmed through UI
- [ ] `dotnet test` passes (existing 44 tests + new bUnit component tests)
