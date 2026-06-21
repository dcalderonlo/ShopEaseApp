## Exploration: blazor-storefront

### Current State

ShopEaseApp is a .NET 10 Minimal API using Vertical Slice (Feature Folders) architecture. Each feature owns its Endpoints, Handler, Models, and Validators. Service registration is manual in `Program.cs` (scoped handlers and services). Authentication uses ASP.NET Core Identity with JWT Bearer + HttpOnly cookie dual auth (`auth_token` cookie). Caching uses Redis + In-Memory. Tests run via xUnit with `WebApplicationFactory<Program>` and EF Core InMemory provider.

Key handlers/services already registered in DI:
- `ProductHandler`, `CategoryHandler` — catalog queries/commands
- `CartService` — Redis-backed cart operations
- `OrderHandler` — checkout and order history
- `RegisterHandler`, `LoginHandler`, `JwtService` — identity

All endpoints implement `IEndpointDefinition` and are auto-discovered at startup.

### Affected Areas

- `src/ShopEaseApp.Api/Program.cs` — must add Blazor Server services and middleware
- `src/ShopEaseApp.Api/ShopEaseApp.Api.csproj` — must add Blazor SDK package references
- `src/ShopEaseApp.Api/Features/{Cart,Catalog,Orders,Identity}/` — handlers are reused directly by Blazor components
- `src/ShopEaseApp.Api/Infrastructure/Auth/` — cookie/JWT auth must flow into Blazor circuits
- `tests/ShopEaseApp.Tests/` — existing tests unaffected; new bUnit test project likely needed
- New directories:
  - `Components/` — shared Blazor components (App, Layout, NavMenu)
  - `Features/Catalog/Components/` — ProductList, ProductDetail
  - `Features/Cart/Components/` — CartView
  - `Features/Orders/Components/` — OrderHistory, Checkout
  - `Features/Identity/Components/` — Login, Register
  - `Pages/` — routeable pages mapping to components

### Approaches

1. **(A) Integrate Blazor Server into the existing API project** — Add `AddServerSideBlazor()`, `MapBlazorHub()`, and `MapRazorComponents<App>()` directly into `ShopEaseApp.Api`. Blazor components live in the same assembly and share the exact same DI container.
   - Pros: Same DI container — inject `ProductHandler`, `CartService`, etc. directly; cookie auth works natively via `HttpContext`; no HTTP serialization overhead; single deployable; minimal project structure change.
   - Cons: UI and API concerns live in one project; larger assembly; risk of tight coupling if components call DbContext directly instead of handlers.
   - Effort: Medium

2. **(B) Separate `ShopEaseApp.Web` Blazor project referencing API assembly** — New web project references `ShopEaseApp.Api`. Re-registers the same services/handlers in its own `Program.cs`.
   - Pros: Clean UI/API separation; smaller focused projects; could later swap frontend tech without touching API.
   - Cons: Must duplicate or extract service registration logic; `WebApplicationFactory` tests become more complex (two entry points); still requires shared DbContext/Redis/Identity configuration; not truly independent since it calls services directly.
   - Effort: Medium-High

3. **(C) Separate Blazor project calling API via HTTP client** — New web project makes HTTP calls to existing API endpoints.
   - Pros: True frontend/backend decoupling; API remains pure REST; could host on separate origin.
   - Cons: Auth is painful — Blazor Server must proxy cookies or manage tokens; serialization overhead on every call; duplicates request/response models; harder to share validation logic; negates the primary benefit of Blazor Server (direct server-side execution).
   - Effort: High

### Recommendation

**Choose Approach A — integrate Blazor Server into `ShopEaseApp.Api`.**

Rationale: The user explicitly noted Blazor Server "can share the same DI container, services, and auth as the existing API." Approach A is the only one that realizes this benefit fully. The project already uses manual DI registration, so adding Blazor services is additive and non-destructive. To mitigate the single-project concern, organize Blazor artifacts by feature (mirroring the existing vertical slice structure) rather than a top-level `Components/` dump.

Auth flow in Blazor Server:
- Implement a custom `AuthenticationStateProvider` that reads `HttpContext.User` from the incoming request and propagates it into the Blazor circuit.
- The existing `auth_token` HttpOnly cookie continues to authenticate both API endpoints and Blazor pages; no token management UI code needed.
- Use `[CascadingAuthenticationState]` and `<AuthorizeView>` for role-based UI rendering.

Shared service reuse:
- Inject `ProductHandler`, `CartService`, `OrderHandler`, `UserManager<AppUser>` directly into Razor components.
- Keep the rule: **components call handlers, not DbContext directly** — preserves the existing boundary.

### Risks

- **Circuit vs. Request scope mismatch**: Blazor Server uses circuit scopes, not request scopes. Scoped services like `AppDbContext` or handlers will live for the lifetime of the circuit. This is acceptable for read operations but can cause stale state; ensure no long-lived scoped service caches unintended state.
- **Prerendering auth gap**: During prerender, `HttpContext` may be null or disconnected. The custom `AuthenticationStateProvider` must handle this gracefully.
- **Test impact**: `WebApplicationFactory<Program>` will now also bootstrap Blazor. Existing integration tests should still pass (endpoints remain), but the factory may need `AddServerSideBlazor()` services configured for InMemory/Testing. A new bUnit test project should be created for component tests.
- **Binary size**: Adding Blazor Server increases the deployed artifact size and memory footprint per circuit.

### Ready for Proposal

**Yes.** The orchestrator should tell the user:

> "Blazor Server will be added directly into the existing API project, sharing DI and cookie auth. We'll organize components by vertical slice (Catalog, Cart, Orders, Identity) to match the existing architecture. The next step is to draft the proposal with page inventory, auth state provider design, and test strategy."
