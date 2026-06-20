## Exploration: shop-bootstrap

### Current State
This is a greenfield project. No .sln or .csproj files currently exist. The technology stack and requirements have been verbally defined (.NET 10+, Minimal APIs, EF Core, SQL Server, Redis, Vertical Slice Architecture). `openspec/config.yaml` is initialized and confirms the stack, architecture, and testing guidelines (xUnit, TDD).

### Affected Areas
- `/ShopEaseApp.sln` — Foundation of the solution
- `/src/ShopEaseApp.Api/*` — Main application project, configuration, and feature folders
- `/tests/ShopEaseApp.Tests/*` — Unit and integration test project

### Approaches

1. **Single-Project API + Tests** — (Everything in `ShopEaseApp.Api`, organized by Feature folders)
   - Pros: Highly cohesive, perfectly aligns with Vertical Slice Architecture. Everything related to a feature (endpoint, logic, data access) lives in one folder. Minimal ceremony and build overhead.
   - Cons: No compile-time enforcement of layer boundaries (relies on team discipline to avoid cross-feature coupling).
   - Effort: Low

2. **Clean Architecture / Multi-Project** — (`Domain`, `Application`, `Infrastructure`, `Api`)
   - Pros: Strict layer boundaries enforced by the compiler.
   - Cons: Directly contradicts the goal of Vertical Slice Architecture by forcing technical grouping over feature grouping. Requires significant boilerplate for a new project.
   - Effort: High

### Recommendation
**Approach 1: Single-Project API + Tests**. This is the idiomatic way to build Minimal APIs using Vertical Slice Architecture in modern .NET. 

**Domain Features Identified (Core Slices):**
- **Identity**: Registration, Login (JWT + Cookies), User Management.
- **Catalog**: Products, Categories, Product Variants (e.g., colors). Read-heavy (uses Output Cache / In-Memory Cache).
- **Cart**: Cart management using Redis (survives browser restarts, volatile data).
- **Orders**: Order creation (checkout summary), inventory decrementing, order history.

### Risks
- **Feature Coupling**: Without strict project boundaries, developers might accidentally reference handlers or models from one feature in another feature. (Mitigation: architecture tests using NetArchTest or strict code review).
- **Inventory Concurrency**: Simple decrement logic on order confirmation could lead to overselling if traffic spikes. (Mitigation: Acceptable for v1 as per user constraints, but should be documented for future extensibility).

### Ready for Proposal
Yes