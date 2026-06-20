# Design: ShopEaseApp — Initial System Bootstrap

## Technical Approach

Implement the online accessories store using a Vertical Slice Architecture within a single .NET 10 Minimal API project. Features (Identity, Catalog, Cart, Orders) are isolated into independent slices owning their request/response DTOs, handlers, endpoint registration, FluentValidation rules, and Entity Framework Core queries. Shared cross-cutting concerns (Auth, DbContext, Caching, Resilience) are centralized in the `Infrastructure/` directory.

## Architecture Decisions

### Decision: Solution Structure & Feature Isolation

| Option | Tradeoff | Choice |
|--------|----------|--------|
| Clean / Onion Architecture | High boilerplate, cross-layer mapping overhead, forces unnatural abstractions. | **Rejected** |
| Vertical Slice Architecture (Minimal APIs) | High cohesion, low coupling between features, easier to maintain and test independently. | **Chosen** |

**Rationale**: Vertical slicing allows each feature to evolve independently, matching the Minimal API pattern seamlessly and preventing the "repository pattern" bloat.

### Decision: Database Access Pattern

| Option | Tradeoff | Choice |
|--------|----------|--------|
| Repository / Unit of Work Pattern | Abstract but redundant with EF Core; hides EF capabilities. | **Rejected** |
| Direct DbContext in Handlers | Ties handlers to EF Core but reduces indirection and leverages EF's built-in UoW. | **Chosen** |

**Rationale**: EF Core's `DbContext` is already a Unit of Work and `DbSet` is a repository. Using it directly in handlers with Fluent API configurations in `Infrastructure/Data/Configurations/` keeps models clean (no data annotations).

### Decision: Authentication Mechanism

| Option | Tradeoff | Choice |
|--------|----------|--------|
| JWT Bearer Only | Vulnerable to XSS if stored in localStorage; harder to manage session natively in web clients. | **Rejected** |
| Dual JWT + HttpOnly Cookie | More complex setup (middleware checks cookie then fallback to header), but vastly more secure for web clients. | **Chosen** |

**Rationale**: Returning the JWT in the response body supports mobile/API clients, while simultaneously setting it in an `HttpOnly`, `Secure`, `SameSite=Strict` cookie secures browser sessions natively. No refresh tokens are used in v1.

### Decision: Caching Strategy

| Option | Tradeoff | Choice |
|--------|----------|--------|
| OutputCache Only | Great for HTTP endpoints but doesn't solve domain-level or persistent caching. | **Rejected** |
| Multi-tier Caching (Output, In-Memory, Redis) | More moving parts but applies the right tool to the right problem. | **Chosen** |

**Rationale**: 
- `OutputCache` (5-min, vary by query) for public endpoints (GET products/categories).
- `IMemoryCache` (10-min sliding) for category reference data loaded at startup.
- `IDistributedCache` (Redis, 7-day TTL, key `"cart:{userId}"`) for user carts.
- Admin endpoints bypass caching entirely.

### Decision: Resilience

| Option | Tradeoff | Choice |
|--------|----------|--------|
| No resilience | Brittle system, failing immediately on transient errors. | **Rejected** |
| Polly Policies | Adds dependency and setup time, but ensures high availability. | **Chosen** |

**Rationale**: Use Polly `HttpClient` factory with exponential backoff (retry 3x) for outbound calls, and a Circuit Breaker for Redis (fallback returns an empty cart and logs warning).

## Data Flow

Order Creation Flow (Cart to Order)

```text
  Client       Order Endpoint     Order Handler       EF Core / DbContext      Redis (Cart)
    │                │                  │                     │                     │
    ├─(1) POST───────►                  │                     │                     │
    │  /api/orders   ├─(2) Validate────►│                     │                     │
    │                │                  ├─(3) Get Cart───────►│                     │
    │                │                  │                     │◄──────(Cart JSON)───┤
    │                │                  ├─(4) Verify Stock───►│                     │
    │                │                  │                     │                     │
    │                │                  ├─(5) Create Order & ─►                     │
    │                │                  │     Decrement Stock │                     │
    │                │                  │◄────(Save.Changes)──┤                     │
    │                │                  │                     │                     │
    │                │                  ├─(6) Clear Cart─────►│                     │
    │                │                  │                     │◄──────(Clear OK)────┤
    │◄─(7) 200 OK────┴──────────────────┤                     │                     │
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `ShopEaseApp.sln` | Create | Root solution file. |
| `src/ShopEaseApp.Api/Program.cs` | Create | Application entry point, Minimal API pipeline, container setup. |
| `src/ShopEaseApp.Api/docker-compose.yml` | Create | Docker compose for Redis and SQL Server development. |
| `src/ShopEaseApp.Api/Features/Identity/Login/LoginEndpoint.cs` | Create | Login route registration and DTOs. |
| `src/ShopEaseApp.Api/Features/Identity/Login/LoginHandler.cs` | Create | Validates user, issues JWT, sets HttpOnly cookie. |
| `src/ShopEaseApp.Api/Features/Orders/Create/CreateOrderEndpoint.cs` | Create | Minimal API route for order creation. |
| `src/ShopEaseApp.Api/Features/Orders/Create/CreateOrderHandler.cs` | Create | Fetches cart, verifies stock, saves order, clears cart. |
| `src/ShopEaseApp.Api/Infrastructure/Data/AppDbContext.cs` | Create | Core DbContext inheriting `IdentityDbContext<AppUser>`. |
| `src/ShopEaseApp.Api/Infrastructure/Auth/AuthSetup.cs` | Create | Registers JWT and Cookie auth middleware configuration. |
| `tests/ShopEaseApp.Tests/Features/Orders/CreateOrderTests.cs` | Create | Unit tests using in-memory EF provider and mock Redis. |

*(Note: Represents a subset of the full vertical slice scaffolding)*

## Interfaces / Contracts

```csharp
// Example Feature Contract (Vertical Slice)
public record CreateOrderRequest(); // Body empty, inferred from User Context
public record CreateOrderResponse(Guid OrderId, decimal Total);

public class CreateOrderValidator : AbstractValidator<CreateOrderRequest> { ... }

// Endpoints use IEndpointDefinition convention
public interface IEndpointDefinition
{
    void MapEndpoints(IEndpointRouteBuilder app);
}
```

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Unit | Handlers, Validators, Domain logic | Direct instantiation of Handlers using EF Core In-Memory provider and Moq for distributed caches/Polly. |
| Integration | API Endpoints, Auth pipeline, DB access | `WebApplicationFactory<Program>` with SQL LocalDB or Testcontainers for real I/O. |
| E2E | N/A | Not configured for this phase. |

## Migration / Rollout

No migration required. Greenfield project.
Initial DB schema will be generated via `dotnet ef migrations add InitialCreate` and applied at startup in the Development environment. Role seeding (Admin, Customer) runs on application startup.

## Open Questions

- None. All architectural and operational decisions are confirmed.
