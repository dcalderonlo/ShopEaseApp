## Exploration: admin-dashboard

### Current State
`ProductVariant` tracks `Name`, `Price`, and `Stock`. There is no per-variant minimum stock threshold or computed inventory status. The catalog API returns raw variant data; stock semantics are client-concern only. Admin operations (create/update/delete products) already exist under `/api/products` with `[Admin]` role gating. The app uses Blazor SSR with interactive server render mode, JWT + cookie dual auth, and Identity roles (`Admin`, `Customer`).

### Affected Areas
- `Domain/ProductVariant.cs` — add `MinimumStockLevel` (int, default 5)
- `Infrastructure/Data/Configurations/ProductVariantConfiguration.cs` — configure new field and default value
- `Features/Catalog/Products/ProductModels.cs` — add `MinimumStockLevel` to `VariantSummary` and `CreateVariantRequest`
- `Features/Catalog/Products/ProductHandler.cs` — map new field on create; note `UpdateProductRequest` does NOT touch variants today (variant edits are out of scope in current handler)
- **New** `Features/Admin/Dashboard/` — handler, endpoints, models for `/api/admin/dashboard` and `/api/admin/products`
- **New** `Components/Pages/AdminDashboard.razor` — `@page "/admin"` with `[Authorize(Roles="Admin")]`
- `Components/Layout/NavMenu.razor` — conditional admin link
- **Migration** — EF Core migration for `ProductVariant.MinimumStockLevel`

### Approaches
1. **Computed status in API layer** — calculate `StockStatus` on read based on `Stock` vs `MinimumStockLevel`; do not persist status in DB.
   - Pros: single source of truth, no sync risk, trivial to change thresholds later
   - Cons: slightly more CPU per query (negligible for this scale)
   - Effort: Low

2. **Persist status in DB** — add a `StockStatus` string/int column updated by triggers or application code.
   - Pros: faster raw reads if millions of rows
   - Cons: denormalized, risk of drift, unnecessary complexity
   - Effort: Medium

### Recommendation
Use Approach 1. Add `MinimumStockLevel` to `ProductVariant` with Fluent default `5`, derive status inline in queries:
- `Stock > MinimumStockLevel` → `"In Stock"`
- `Stock > 0 && Stock <= MinimumStockLevel` → `"Low Stock"`
- `Stock == 0` → `"Out of Stock"`

Build a dedicated `AdminDashboardHandler` that performs aggregate queries (`Count`, `Sum(Price * Stock)`) with `AsNoTracking()`. Map new admin endpoints under `/api/admin/*` and require `Admin` role. Create the Blazor page with filter chips bound to a query parameter or local state.

### Risks
- `UpdateProductRequest` currently does not support variant edits; the admin table’s "edit" action may need a separate variant-level endpoint later.
- Aggregate query `Sum(Price * Stock)` could overflow if inventory is massive; `decimal` is safe for this domain.
- `[Authorize(Roles="Admin")]` on Blazor pages requires a working `CascadingAuthenticationState`; the existing `Routes.razor` already wraps the router.

### Ready for Proposal
Yes. Proceed to proposal. The scope is well-defined: one schema change, one new feature slice (Admin/Dashboard), one Blazor page, and one EF migration.
