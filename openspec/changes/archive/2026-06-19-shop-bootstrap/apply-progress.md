# Apply Progress: shop-bootstrap

**Mode**: Strict TDD — `dotnet test` (xUnit)  
**Delivery**: Feature-branch-chain (5 work units)  
**Date**: 2026-06-19  
**Status**: All phases complete — 45/45 tests GREEN, 0 build warnings

## TDD Cycle Evidence

Every handler, validator, and service followed RED → GREEN → TRIANGULATE → REFACTOR.

### Phase 1 — Infrastructure (3 tests)

| Task | Test File | Layer | Safety Net | RED | GREEN | TRIANGULATE | REFACTOR |
|------|-----------|-------|------------|-----|-------|-------------|----------|
| RoleSeeder | `RoleSeederTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ 2 cases | ✅ Clean |
| IEndpointDefinition | structural | — | N/A | ➖ Structural | ✅ Build pass | ➖ Structural | ✅ Clean |
| docker-compose | structural | — | N/A | ➖ Structural | ✅ Config | ➖ Structural | ✅ Clean |

### Phase 2 — Identity (15 tests)

| Task | Test File | Layer | Safety Net | RED | GREEN | TRIANGULATE | REFACTOR |
|------|-----------|-------|------------|-----|-------|-------------|----------|
| RegisterValidator | `RegisterValidatorTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ 6 cases | ✅ Clean |
| LoginValidator | `LoginValidatorTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ 4 cases | ✅ Clean |
| RegisterHandler (success) | `RegisterHandlerTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ 2 cases | ✅ Clean |
| RegisterHandler (duplicate) | `RegisterHandlerTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ 2 cases | ✅ Clean |
| Auth flow (register→login) | `AuthIntegrationTests.cs` | Integration | N/A (new) | ✅ Written | ✅ Passed | ✅ Cookie+token verified | ✅ Clean |
| Login failure | `AuthIntegrationTests.cs` | Integration | N/A (new) | ✅ Written | ✅ Passed | ✅ Invalid creds tested | ✅ Clean |
| Admin role denial | `AuthIntegrationTests.cs` | Integration | N/A (new) | ✅ Written | ✅ Passed | ✅ Customer→403/404 | ✅ Clean |

### Phase 3 — Catalog (13 tests)

| Task | Test File | Layer | Safety Net | RED | GREEN | TRIANGULATE | REFACTOR |
|------|-----------|-------|------------|-----|-------|-------------|----------|
| CategoryHandler: browse | `CategoryHandlerTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ 2 cases (populated+empty) | ✅ Clean |
| CategoryHandler: create | `CategoryHandlerTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Persistence verified | ✅ Clean |
| CategoryHandler: delete blocked | `CategoryHandlerTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Products assigned check | ✅ Clean |
| CategoryHandler: delete unknown | `CategoryHandlerTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Edge case 404 | ✅ Clean |
| ProductHandler: browse | `ProductHandlerTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Variants included | ✅ Clean |
| ProductHandler: empty list | `ProductHandlerTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Empty triangulation | ✅ Clean |
| ProductHandler: create | `ProductHandlerTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Variants+stock | ✅ Clean |
| ProductHandler: not found | `ProductHandlerTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Null check | ✅ Clean |
| ProductHandler: update fail | `ProductHandlerTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Unknown ID | ✅ Clean |
| Guest browse | `CatalogIntegrationTests.cs` | Integration | N/A (new) | ✅ Written | ✅ Passed | ✅ Public access | ✅ Clean |
| Customer denied create | `CatalogIntegrationTests.cs` | Integration | N/A (new) | ✅ Written | ✅ Passed | ✅ Role enforcement | ✅ Clean |
| Empty categories | `CatalogIntegrationTests.cs` | Integration | N/A (new) | ✅ Written | ✅ Passed | ✅ Graceful empty | ✅ Clean |

### Phase 4 — Cart (7 tests)

| Task | Test File | Layer | Safety Net | RED | GREEN | TRIANGULATE | REFACTOR |
|------|-----------|-------|------------|-----|-------|-------------|----------|
| CartService: view | `CartServiceTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Items+total | ✅ Clean |
| CartService: add | `CartServiceTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Price snapshot | ✅ Clean |
| CartService: unknown variant | `CartServiceTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Rejection tested | ✅ Clean |
| CartService: update qty | `CartServiceTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Snapshot preserved | ✅ Clean |
| CartService: remove | `CartServiceTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Selective removal | ✅ Clean |
| CartService: clear | `CartServiceTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Empty after clear | ✅ Clean |
| CartService: isolation | `CartServiceTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ User isolation | ✅ Clean |

### Phase 5 — Orders (7 tests)

| Task | Test File | Layer | Safety Net | RED | GREEN | TRIANGULATE | REFACTOR |
|------|-----------|-------|------------|-----|-------|-------------|----------|
| OrderHandler: checkout success | `OrderHandlerTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Stock decrement+cart clear | ✅ Clean |
| OrderHandler: insufficient stock | `OrderHandlerTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Stock preserved+cart kept | ✅ Clean |
| OrderHandler: mixed cart rejection | `OrderHandlerTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ No partial decrement | ✅ Clean |
| OrderHandler: empty cart | `OrderHandlerTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Rejection message | ✅ Clean |
| OrderHandler: view own order | `OrderHandlerTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Owner access | ✅ Clean |
| OrderHandler: cross-user denial | `OrderHandlerTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Null on cross-access | ✅ Clean |
| OrderHandler: empty history | `OrderHandlerTests.cs` | Unit | N/A (new) | ✅ Written | ✅ Passed | ✅ Empty list | ✅ Clean |

## Test Summary
- **Total tests written**: 45
- **Total tests passing**: 45
- **Layers used**: Unit (38), Integration (7), E2E (0)
- **Approval tests**: None — all greenfield, no refactoring of existing code
- **Pure functions created**: N/A — .NET handlers use DI; validators are functional pipelines
- **Test files**: 11 (RoleSeederTests, RegisterValidatorTests, LoginValidatorTests, RegisterHandlerTests, AuthIntegrationTests, CategoryHandlerTests, ProductHandlerTests, CatalogIntegrationTests, CartServiceTests, OrderHandlerTests, ShopEaseTestFactory)

## Gotchas Discovered During Implementation

1. **EF Core InMemory dual-provider fix**: `WebApplicationFactory` must use `ConfigureWebHost` with `builder.UseEnvironment("Testing")`, remove only `DbContextOptions<T>`, and NOT use `EnableServiceProviderCaching(false)` — it isolates InMemory instances and breaks shared state.
2. **EF Core InMemory no transactions**: `BeginTransactionAsync` is unsupported by InMemory provider. Using `SaveChangesAsync` atomicity instead.
3. **`.WithOpenApi()` deprecated in .NET 10**: Removed from all endpoint registrations.
4. **`AddStandardResilienceHandler` requires `IHttpClientBuilder`**: Must chain from `AddHttpClient("name")`, not from `IServiceCollection` directly.
5. **Test factory DB isolation**: `ShopEaseTestFactory` uses `Guid.NewGuid()` per instance to prevent test class collisions on shared InMemory database.
6. **Role seeding in tests**: Must run inside the app (Program.cs startup) via try-catch so InMemory DB is seeded in the same provider scope as requests.

## Phase Summary

| Phase | Capability | Tests Written | Status |
|-------|-----------|---------------|--------|
| 1 | Infrastructure | 3 | ✅ Complete |
| 2 | Identity | 15 | ✅ Complete |
| 3 | Catalog | 13 | ✅ Complete |
| 4 | Cart | 7 | ✅ Complete |
| 5 | Orders | 7 | ✅ Complete |
| **Total** | **5 capabilities** | **45** | **All GREEN** |
