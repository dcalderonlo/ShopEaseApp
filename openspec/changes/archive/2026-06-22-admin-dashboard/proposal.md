# Proposal: admin-dashboard

## Intent

Admins currently lack visibility into inventory health and a centralized UI to manage products. This change adds a dedicated admin dashboard with inventory metrics and a Blazor page for browsing, filtering, and editing products.

## Scope

### In Scope
- Add `MinimumStockLevel` (int, default 5) to `ProductVariant` + EF migration
- `GET /api/admin/dashboard` — aggregates: total products, total SKUs, low stock items, inventory value
- `GET /api/admin/products` — full product list with variant-level stock, computed status, price
- Blazor admin page `/admin` `[Authorize(Roles="Admin")]`:
  - Filter chips: All / In Stock / Low Stock / Out of Stock
  - Summary cards: total products, total SKUs, low stock count, inventory value
  - Products table: Product Name, Category, Stock, Status badge, Price, Actions
  - Edit modal: inline form for name, description, category, image URLs
  - Delete modal: confirmation dialog
- `NavMenu`: conditionally render "Admin" link for Admin role

### Out of Scope
- Variant-level CRUD from admin UI
- Product creation from admin page
- Bulk operations, exports, reports

## Capabilities

### New Capabilities
- `admin-dashboard`: Admin panel with dashboard metrics, product table, filter chips, edit/delete modals

### Modified Capabilities
- `catalog`: `ProductVariant` entity gains `MinimumStockLevel`; variant summaries include derived stock status

## Approach

Compute `StockStatus` in the read layer:
- `Stock > MinimumStockLevel` → `In Stock`
- `Stock > 0 && Stock <= MinimumStockLevel` → `Low Stock`
- `Stock == 0` → `Out of Stock`

Use `AsNoTracking()` aggregate queries for dashboard metrics. Add dedicated `AdminDashboardHandler` under `Features/Admin/Dashboard/`. Blazor page binds filter chips to local state with a queryable product list. `MinimumStockLevel` defaults to `5` via Fluent Configuration.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `Domain/ProductVariant.cs` | Modified | Add `MinimumStockLevel` property |
| `Infrastructure/Data/Configurations/ProductVariantConfiguration.cs` | Modified | Configure default `5` |
| `Features/Catalog/Products/ProductModels.cs` | Modified | Add `MinimumStockLevel` to `VariantSummary` and `CreateVariantRequest` |
| `Features/Catalog/Products/ProductHandler.cs` | Modified | Map new field on create |
| `Features/Admin/Dashboard/` | New | Handler, endpoints, models for admin API |
| `Components/Pages/AdminDashboard.razor` | New | Admin page with filters, table, modals |
| `Components/Layout/NavMenu.razor` | Modified | Conditional Admin link |
| `Infrastructure/Data/Migrations/` | New | EF Core migration for new field |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| `UpdateProductRequest` does not support variant edits today | Med | Admin edit modal limits fields to product-level only; variant edits deferred |
| Aggregate `Sum(Price * Stock)` could overflow | Low | Domain uses `decimal`; SQL Server handles scale |
| Blazor auth state not cascading correctly | Low | Existing `Routes.razor` already wraps router; verify role claim is present |

## Rollback Plan

1. Revert EF migration: `dotnet ef migrations remove`
2. Remove `Features/Admin/` folder and endpoint registrations
3. Remove `AdminDashboard.razor`
4. Revert `NavMenu.razor` conditional link

## Dependencies

- Existing Identity role system (`Admin` role) and JWT/cookie auth
- Existing catalog endpoints for product creation (out of scope here)

## Success Criteria

- [ ] `MinimumStockLevel` present in `ProductVariant` with default `5`
- [ ] `/api/admin/dashboard` returns accurate aggregates
- [ ] `/api/admin/products` returns products with computed stock status badges
- [ ] `/admin` page loads only for users with `Admin` role
- [ ] Filter chips update table to show matching stock statuses
- [ ] Edit modal persists product-level changes; delete modal removes product
