# Tasks: Admin Dashboard

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | ~350 |
| 400-line budget risk | Low |
| Chained PRs recommended | No |
| Suggested split | Single PR |
| Delivery strategy | ask-on-risk |
| Chain strategy | single-pr-default |

Decision needed before apply: No
Chained PRs recommended: No
Chain strategy: single-pr-default
400-line budget risk: Low

## Phase 1: Domain + Schema

- [x] 1.1 Add `int MinimumStockLevel { get; set; }` to `Domain/ProductVariant.cs`
- [x] 1.2 Add `.Property(v => v.MinimumStockLevel).HasDefaultValue(5)` to `ProductVariantConfiguration.cs`
- [x] 1.3 Add `MinimumStockLevel` to `VariantSummary` (positional) and `CreateVariantRequest` (optional, default 5) in `ProductModels.cs`
- [x] 1.4 Map `MinimumStockLevel` in `ProductHandler.ToResponse()` and `CreateAsync()` variant creation
- [x] 1.5 Run `dotnet ef migrations add AddMinimumStockLevel`

## Phase 2: Admin API

- [x] 2.1 Create `Features/Admin/Dashboard/AdminModels.cs` — `DashboardMetrics`, `AdminProductResponse`, `AdminVariantSummary` with `StockStatus`
- [x] 2.2 Create `AdminDashboardHandler.cs` — `GetMetricsAsync()` (aggregate queries) + `GetProductsAsync(string? status)` with `ComputeStatus()` helper
- [x] 2.3 Create `AdminEndpoints.cs` — `IEndpointDefinition`, `GET /api/admin/dashboard`, `GET /api/admin/products`, both `.RequireRole("Admin")`
- [x] 2.4 Register `AdminDashboardHandler` and `AdminEndpoints` in `Program.cs`
- [x] 2.5 Unit test: `GetMetricsAsync` returns correct counts (RED→GREEN, in-memory DbContext)
- [x] 2.6 Unit test: `GetProductsAsync` filters by status; `ComputeStatus` boundaries (0, min, min+1)

## Phase 3: Blazor Admin Page

- [x] 3.1 Create `Components/Pages/AdminDashboard.razor` — `@page "/admin"`, `@rendermode InteractiveServer`, `[Authorize(Roles="Admin")]`, summary cards (total products, SKUs, low stock, inventory value)
- [x] 3.2 Add filter chips (All / In Stock / Low Stock / Out of Stock) with local state binding
- [x] 3.3 Build products table with Status badge (colored by status: green/yellow/red)
- [x] 3.4 Create `AdminEditModal.razor` — form: name, description, category dropdown, image URLs, save button → `PUT /api/products/{id}`
- [x] 3.5 Create `AdminDeleteModal.razor` — confirmation dialog with product name → `DELETE /api/products/{id}`
- [x] 3.6 Wire modals to table Actions column (Edit / Delete buttons)
- [x] 3.7 Update `NavMenu.razor` — add `<AuthorizeView Roles="Admin">` with link to `/admin`
- [x] 3.8 bUnit test: AdminDashboard renders summary cards and status-filtered table

## Phase 4: Verify

- [x] 4.1 `dotnet build` — zero warnings
- [x] 4.2 `dotnet test` — all GREEN (existing + new)
- [x] 4.3 Verify migration applied: `dotnet ef database update`
