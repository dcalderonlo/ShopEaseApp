# Tasks: Blazor Server Storefront UI

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | ~300–450 |
| 400-line budget risk | Medium |
| Chained PRs recommended | No |
| Suggested split | Single PR |
| Delivery strategy | ask-on-risk |
| Chain strategy | single-pr-default |

Decision needed before apply: No
Chained PRs recommended: No
Chain strategy: single-pr-default
400-line budget risk: Medium

## Phase 1: Infrastructure — Blazor Bootstrap

- [x] 1.1 Add `Microsoft.AspNetCore.Components.Server` to `ShopEaseApp.Api.csproj`
- [x] 1.2 Wire Blazor Server in `Program.cs`: `AddServerSideBlazor`, `AddRazorComponents().AddInteractiveServerComponents()`, `MapBlazorHub`, `MapRazorComponents<App>`
- [x] 1.3 Create `Components/_Imports.razor` with common namespaces
- [x] 1.4 Create `Components/App.razor` (Router + HeadOutlet + CascadingAuthenticationState)
- [x] 1.5 Create `Components/Routes.razor` (route table)
- [x] 1.6 Create `Components/Layout/MainLayout.razor` (navbar + @Body)
- [x] 1.7 Create `Components/Layout/NavMenu.razor` (links: Catalog, Cart, Login/Register)
- [x] 1.8 Register `IHttpContextAccessor` + `AuthenticationStateProvider` in DI
- [x] 1.9 Create `Infrastructure/Auth/ServerAuthenticationStateProvider.cs` (RED→GREEN)

## Phase 2: Identity UI — Register + Login

- [x] 2.1 Create `Features/Identity/Components/Register.razor` with registration form (RED→GREEN)
- [x] 2.2 Create `Features/Identity/Components/Login.razor` with login form + cookie set (RED→GREEN)
- [x] 2.3 Wire post-login redirect to `/`

## Phase 3: Catalog UI

- [x] 3.1 Create `Features/Catalog/Components/ProductList.razor` — browse all products (RED→GREEN)
- [x] 3.2 Create `Features/Catalog/Components/ProductDetail.razor` — detail + add-to-cart (RED→GREEN)
- [x] 3.3 Wire routes: `/` → ProductList, `/product/{id:int}` → ProductDetail

## Phase 4: Cart UI

- [x] 4.1 Create `Features/Cart/Components/CartPage.razor` — view, update qty, remove (RED→GREEN)
- [x] 4.2 Create `Components/Layout/CartSummary.razor` — item count badge (RED→GREEN)
- [x] 4.3 Create `Features/Cart/Components/Checkout.razor` — create order, show confirmation (RED→GREEN)
- [x] 4.4 Wire routes: `/cart`, `/checkout`

## Phase 5: Orders UI + Auth State

- [x] 5.1 Create `Features/Orders/Components/OrderHistory.razor` (RED→GREEN)
- [x] 5.2 Update NavMenu: auth-aware rendering (Guest→Login/Register; Auth→Username+Logout)
- [x] 5.3 Create logout logic: clear cookie, refresh auth state

## Phase 6: Testing + Verification

- [x] 6.1 Create `tests/ShopEaseApp.Blazor.Tests/` with bUnit + xUnit
- [x] 6.2 Add bUnit tests: `ProductList`, `Login`, `CartPage` (≥3 tests)
- [x] 6.3 `dotnet build` — zero warnings
- [x] 6.4 `dotnet test` — 44 existing + new tests all GREEN
