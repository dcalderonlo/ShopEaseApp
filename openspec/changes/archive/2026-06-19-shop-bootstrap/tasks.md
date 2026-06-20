# Tasks: ShopEaseApp — Initial System Bootstrap

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | 1000–1400 (production + test) |
| 400-line budget risk | High |
| Chained PRs recommended | Yes |
| Suggested split | 5 work units (Infrastructure → Identity → Catalog → Cart → Orders) |
| Delivery strategy | ask-on-risk |
| Chain strategy | pending |

Decision needed before apply: Resolved
Chained PRs recommended: Yes
Chain strategy: feature-branch-chain
400-line budget risk: High

### Suggested Work Units

| Unit | Goal | Likely PR | Notes |
|------|------|-----------|-------|
| 1 | Foundation: solution, EF Core, migrations, caching, Polly, Serilog, Scalar | PR 1 | Base = main; enables all features |
| 2 | Identity: Register, Login, Logout, role seeding, JWT + Cookie auth | PR 2 | Base = PR 1; depends on Infrastructure |
| 3 | Catalog: Products, Categories, Variants, public & admin endpoints | PR 3 | Base = PR 2; depends on Identity for admin authorization |
| 4 | Cart: Redis cart service, endpoints, session persistence | PR 4 | Base = PR 3; depends on Identity for user context |
| 5 | Orders: Create order, order summary, history, stock decrement, full checkout | PR 5 | Base = PR 4; depends on Catalog (stock) + Cart (items) |

---

## Phase 1: Foundation — Infrastructure (12 tasks)

- [x] 1.1 Create `ShopEaseApp.sln`, `src/ShopEaseApp.Api`, and `tests/ShopEaseApp.Tests` project structure
- [x] 1.2 Add solution files: `ShopEaseApp.Api.csproj`, `ShopEaseApp.Tests.csproj`
- [x] 1.3 Install NuGet packages: EF Core, SQL Server, Identity, JWT Bearer, Redis, FluentValidation, Serilog, Scalar, Polly
- [x] 1.4 Create `src/ShopEaseApp.Api/docker-compose.yml` for Redis + SQL Server
- [x] 1.5 Create `src/ShopEaseApp.Api/Infrastructure/Data/AppDbContext.cs` inheriting `IdentityDbContext<AppUser>`
- [x] 1.6 Create `src/ShopEaseApp.Api/Infrastructure/Data/Configurations/` base folder; add `IEntityTypeConfiguration<T>` pattern
- [x] 1.7 Add Serilog config in `Program.cs` (console sink for dev, file sink for prod)
- [x] 1.8 Add Scalar endpoint mapping at `/scalar/v1` in `Program.cs`
- [x] 1.9 Add Output Cache + In-Memory Cache + Redis Distributed Cache configuration in `Program.cs`
- [x] 1.10 Add Polly HTTP client + Redis circuit breaker policies in `Program.cs`
- [x] 1.11 Create initial EF Core migration: `dotnet ef migrations add InitialCreate`
- [x] 1.12 Add `Program.cs` startup logic: auto-migrate dev, log to sinks, register auth middleware placeholders

---

## Phase 2: Identity Feature (11 tasks)

- [x] 2.1 Create `src/ShopEaseApp.Api/Infrastructure/Auth/AuthSetup.cs` registering JWT + Cookie auth schemes
- [x] 2.2 Create `AppUser` entity extending `IdentityUser` in `Infrastructure/Auth/AppUser.cs`
- [x] 2.3 Create `AppUserConfiguration` (Fluent API) in `Infrastructure/Data/Configurations/`; add migration
- [x] 2.4 Create role seeding in `Program.cs` (Customer, Admin roles created at startup)
- [x] 2.5 [RED] Write failing unit test: `LoginHandlerTests.Should_Return_401_For_Invalid_Credentials()` in `tests/ShopEaseApp.Tests/Features/Identity/`
- [x] 2.6 Create `Features/Identity/Login/LoginRequest` and `LoginResponse` DTOs
- [x] 2.7 Create `Features/Identity/Login/LoginValidator` extending `AbstractValidator<LoginRequest>`
- [x] 2.8 Create `Features/Identity/Login/LoginHandler` validating credentials, issuing JWT + HttpOnly cookie
- [x] 2.9 [GREEN] Run test from 2.5; implement LoginHandler to pass
- [x] 2.10 [RED] Write failing unit test: `RegisterHandlerTests.Should_Create_Customer_Role_By_Default()`
- [x] 2.11 Create `Features/Identity/Register/` endpoint + handler + validator + DTOs; [GREEN] implement to pass test
- [x] 2.12 Create `Features/Identity/Logout/` endpoint + handler + unit test (simple session clear)
- [x] 2.13 Create integration test: `AuthFlowTests.Should_Register_Then_Login_Then_Access_Protected_Endpoint()`
- [x] 2.14 Create `Features/Identity/Endpoints.cs` registering all identity routes via `IEndpointDefinition`

---

## Phase 3: Catalog Feature (15 tasks)

- [x] 3.1 Create `Category` entity; add `CategoryConfiguration` (Fluent API); migrate
- [x] 3.2 Create `Product` entity with CategoryId FK; add `ProductConfiguration` (Fluent API); migrate
- [x] 3.3 Create `ProductVariant` entity (variant ID, type/color, price, stock); add `ProductVariantConfiguration`; migrate
- [x] 3.4 [RED] Write failing unit test: `ListProductsHandlerTests.Should_Return_All_Products_With_OutputCache_Header()`
- [x] 3.5 Create `Features/Catalog/ListProducts/` handler + DTO; [GREEN] implement with OutputCache 5-min
- [x] 3.6 Create `Features/Catalog/GetProduct/` handler + DTO + [RED/GREEN] tests
- [x] 3.7 Create `Features/Catalog/ListCategories/` handler + DTO + [RED/GREEN] tests (InMemoryCache 10-min)
- [x] 3.8 [RED] Write failing unit test: `CreateProductHandlerTests.Should_Require_Admin_Role()`
- [x] 3.9 Create `Features/Catalog/CreateProduct/` handler + validator + DTO; [GREEN] implement with Admin authorization
- [x] 3.10 Create `Features/Catalog/UpdateProduct/` handler + validator + DTO + [RED/GREEN] tests
- [x] 3.11 Create `Features/Catalog/DeleteProduct/` handler + [RED/GREEN] tests (verify Admin role)
- [x] 3.12 Create `Features/Catalog/ManageCategories/` (create, update, delete) + validators + [RED/GREEN] tests
- [x] 3.13 Create `Features/Catalog/ManageVariants/` (nested under product) + [RED/GREEN] tests
- [x] 3.14 Create `Features/Catalog/Endpoints.cs` registering all catalog routes
- [x] 3.15 Create integration test: `CatalogAuthTests.Should_Allow_Guest_Browse_But_Deny_Admin_Create()`

---

## Phase 4: Cart Feature (8 tasks)

- [x] 4.1 Create `Features/Cart/Models/CartItem.cs` (variant ID, quantity, price snapshot) — NOT an EF entity
- [x] 4.2 Create `Features/Cart/Services/RedisCartService.cs` with methods: GetCart, AddItem, UpdateQuantity, RemoveItem, ClearCart
- [x] 4.3 Register `IRedisCartService` in `Program.cs` with Polly circuit breaker fallback
- [x] 4.4 [RED] Write failing unit test: `RedisCartServiceTests.Should_Get_Empty_Cart_For_New_User()`
- [x] 4.5 [GREEN] Implement RedisCartService to pass tests; mock Redis client with Moq
- [x] 4.6 Create `Features/Cart/Endpoints/` (ViewCart, AddItem, UpdateQuantity, RemoveItem) + DTOs + validators
- [x] 4.7 [RED/GREEN] Write + implement unit tests for each endpoint (auth required, user isolation)
- [x] 4.8 Create integration test: `CartPersistenceTests.Should_Restore_Cart_After_Session_Restart()` (verify Redis TTL)

---

## Phase 5: Orders Feature (12 tasks)

- [x] 5.1 Create `Order` entity (OrderId, UserId, Total, Status, CreatedAt); add `OrderConfiguration` (Fluent API)
- [x] 5.2 Create `OrderItem` entity (OrderItemId, OrderId, VariantId, Quantity, UnitPrice); add `OrderItemConfiguration`; migrate
- [x] 5.3 [RED] Write failing unit test: `CreateOrderHandlerTests.Should_Reject_Order_When_Stock_Insufficient()`
- [x] 5.4 Create `Features/Orders/Create/CreateOrderHandler` reading cart, validating stock, creating order, decrementing stock, clearing cart (transactional)
- [x] 5.5 [GREEN] Implement CreateOrderHandler to pass test; verify transaction rollback on stock check failure
- [x] 5.6 Create `Features/Orders/Create/` DTO + validator + endpoint
- [x] 5.7 [RED] Write failing unit test: `CreateOrderHandlerTests.Should_Clear_Cart_After_Successful_Order()`
- [x] 5.8 [GREEN] Verify cart service is called to clear cart in CreateOrderHandler
- [x] 5.9 Create `Features/Orders/GetOrderSummary/` handler + DTO + [RED/GREEN] tests (customer can view own orders only)
- [x] 5.10 Create `Features/Orders/GetCustomerOrderHistory/` handler + DTO + [RED/GREEN] tests
- [x] 5.11 Create `Features/Orders/GetAdminOrderList/` handler + DTO + [RED/GREEN] tests (Admin role required, 403 for Customer)
- [x] 5.12 Create `Features/Orders/Endpoints.cs` registering all order routes
- [x] 5.13 Create integration test: `CheckoutFlowTests.Should_Complete_Full_Checkout_Cart_To_Order_To_History()`

---

## Phase 6: Verification (5 tasks)

- [x] 6.1 Run `dotnet build` — verify zero warnings
- [x] 6.2 Run `dotnet test` — verify all unit + integration tests pass
- [x] 6.3 Run `dotnet test /p:CollectCoverage=true` — verify coverage above 75%
- [x] 6.4 Verify Scalar UI at `/scalar/v1` renders all endpoints with auth schemes
- [x] 6.5 Verify 403 on admin endpoints when authenticated as Customer role; 200 as Admin

---

## Summary

| Phase | Tasks | Focus |
|-------|-------|-------|
| Phase 1 | 12 | Foundation: projects, EF Core, migrations, caching, Polly, Serilog, Scalar |
| Phase 2 | 14 | Identity: Register, Login, Logout, role seeding, JWT + Cookie auth (with TDD) |
| Phase 3 | 15 | Catalog: Products, Categories, Variants, endpoints, admin CRUD (with TDD + Output/In-Memory cache) |
| Phase 4 | 8 | Cart: Redis cart service, endpoints, session persistence (with TDD + Polly fallback) |
| Phase 5 | 12 | Orders: Create order, history, admin list, stock decrement, checkout flow (with TDD + transactional) |
| Phase 6 | 5 | Verification: build, tests, coverage, Scalar, role enforcement |
| **Total** | **66** | **Greenfield bootstrap with strict TDD (RED → GREEN for all handlers)** |

---

## Key Implementation Notes

### TDD Workflow (Mandatory — strict_tdd: true)
For every handler, validator, and service:
1. **[RED]**: Write a failing unit test first
2. **[GREEN]**: Implement minimal code to pass test
3. **[REFACTOR]**: Clean up (not a separate task, but inline)

### Dependency Order
- Phase 1 must complete before Phases 2–5 can start (unblocks Program.cs, DbContext, auth middleware)
- Phase 2 (Identity) must complete before Phase 3 (Catalog admin routes need Auth)
- Phase 3 (Catalog) and Phase 4 (Cart) can proceed in parallel after Phase 2
- Phase 5 (Orders) requires both Phase 3 (stock) and Phase 4 (cart items)
- Phase 6 (Verification) runs after all implementation is complete

### Caching Strategy
- **OutputCache** (5-min, vary by query) → ListProducts, ListCategories
- **IMemoryCache** (10-min sliding) → Category reference data at startup
- **IDistributedCache (Redis)** (7-day TTL, key: `"cart:{userId}"`) → Cart persistence

### Polly Resilience
- **HTTP Retry**: 3 retries with exponential backoff for outbound calls
- **Redis Circuit Breaker**: Fallback to IMemoryCache if Redis unavailable; cart returns empty, logs warning

### Auth & Authorization
- **Public**: GET /products, GET /categories (no auth required)
- **Customer-only**: POST /cart/*, POST /orders/*, GET /orders/history
- **Admin-only**: POST/PUT/DELETE /products, POST/PUT/DELETE /categories, GET /orders/admin
- **Dual Auth**: Login response includes JWT in body + HttpOnly cookie in Set-Cookie header

---

## Estimated Workload

- **Production Code**: ~700–900 lines (entities, handlers, validators, DTOs, services)
- **Test Code**: ~300–500 lines (unit + integration tests covering all handlers and critical paths)
- **Configuration**: ~100–150 lines (Program.cs, DbContext, Polly, Serilog, auth middleware)
- **Total**: ~1,000–1,400 lines
- **Risk**: HIGH — exceeds 400-line review budget; chained PRs recommended (5 work units per phase group)
