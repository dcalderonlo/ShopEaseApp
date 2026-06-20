# Proposal: ShopEaseApp — Initial System Bootstrap

## Intent

Bootstrap the complete online accessories store system: solution structure, infrastructure, and all core domain capabilities. No prior codebase exists.

## Scope

### In Scope
- Solution and project scaffolding (Vertical Slice, single API project + test project)
- Infrastructure setup: EF Core, SQL Server, migrations, Serilog, Scalar, caching layers, Polly
- Identity: registration, login (JWT + HttpOnly cookies), logout, role seeding (Customer, Admin)
- Catalog: products, categories, variants (color/type), admin CRUD, public browsing with caching
- Cart: Redis-backed per-user cart, persistent across sessions, add/remove/update items
- Orders: create order from cart, order summary view, inventory decrement on confirmation, order history

### Out of Scope
- Payment gateway integration (Stripe, PayPal) — deferred
- Real-time stock reservation / concurrency control — deferred
- E2E tests — no runner configured
- Email notifications — deferred
- Admin dashboard analytics — deferred

## Capabilities

### New Capabilities
- `infrastructure`: Solution bootstrap, EF Core + Fluent API config, migrations, Serilog, Scalar, caching pipeline, Polly resilience
- `identity`: User registration, login, JWT + HttpOnly cookie issuance, logout, role assignment (Customer / Admin)
- `catalog`: Products CRUD, categories CRUD, product variants, public browsing endpoints, Output Cache + In-Memory Cache
- `cart`: Redis-persistent cart per user, add / remove / update items, cart-to-order handoff
- `orders`: Order creation from cart, order summary view, inventory decrement on confirmation, order history (customer + admin views)

### Modified Capabilities
None — greenfield project.

## Approach

Single-solution, two-project structure (`ShopEaseApp.Api` + `ShopEaseApp.Tests`). Features organized as vertical slices under `Features/` folder. Each slice owns its endpoint, handler/service, models, validators, and EF config. Shared infrastructure (DbContext, Identity, caching, auth) lives in `Infrastructure/`.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `src/ShopEaseApp.Api/` | New | Main API project with all feature slices |
| `src/ShopEaseApp.Api/Features/` | New | Vertical slices: Identity, Catalog, Cart, Orders |
| `src/ShopEaseApp.Api/Infrastructure/` | New | DbContext, Identity, caching, Polly, Serilog config |
| `tests/ShopEaseApp.Tests/` | New | xUnit unit + integration tests mirroring feature slices |
| `ShopEaseApp.sln` | New | Solution file |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Cross-feature coupling in single project | Medium | Architecture tests with NetArchTest; strict code review |
| Redis unavailable in dev environment | Low | Fallback to IMemoryCache via feature flag or Docker Compose |
| Inventory overselling (no concurrency guard) | Low | Acceptable for v1; document as known limitation |
| JWT + Cookie dual auth complexity | Medium | Centralize auth config in `Infrastructure/Auth/`; test both flows |

## Rollback Plan

Greenfield project — no rollback risk. If approach proves unworkable, delete generated files and restart scaffold. No production data at risk.

## Dependencies

- Docker (Redis in development via `docker-compose.yml`)
- SQL Server instance (local or Docker)
- .NET 10 SDK

## Success Criteria

- [ ] `dotnet build` passes with zero warnings
- [ ] `dotnet test` passes with all unit and integration tests green
- [ ] Scalar UI renders all endpoints with auth schemes documented
- [ ] JWT login returns token + sets HttpOnly cookie
- [ ] Cart survives a simulated session restart (Redis persistence verified)
- [ ] Order creation decrements product stock in the database
- [ ] Admin-only endpoints return 403 for Customer role
