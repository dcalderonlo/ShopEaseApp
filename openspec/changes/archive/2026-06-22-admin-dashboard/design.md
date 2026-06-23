# Design: Admin Dashboard

## Technical Approach

Extend the existing Vertical Slice pattern (`Features/{Area}/{Feature}/`) with a new `Admin/Dashboard/` slice. Add `MinimumStockLevel` to the `ProductVariant` entity, compute `StockStatus` as a read-time projection (never persisted), and expose two admin API endpoints. The Blazor page consumes these endpoints via `HttpClient` and manages filter/modal state locally with `@rendermode InteractiveServer`.

## Architecture Decisions

| Decision | Options Considered | Choice | Rationale |
|----------|-------------------|--------|-----------|
| MinimumStockLevel storage | New column vs. appsettings constant | Column on `ProductVariant`, default 5 via Fluent API | Per-variant flexibility; matches existing column pattern (`Stock`, `Price`) |
| StockStatus persistence | DB column vs. computed on read | Computed (switch expression in LINQ projection) | Single source of truth; no drift risk; negligible CPU cost at this scale |
| Admin feature location | Reuse `ProductHandler` vs. new slice | New `Features/Admin/Dashboard/` slice | Separation of concerns; admin queries have different auth and aggregation needs |
| Blazor render mode | SSR vs. InteractiveServer | InteractiveServer for page, SSR for layout | Filter chips and modals require client interactivity |
| Modal pattern | Inline HTML vs. child components | Separate `AdminEditModal.razor` and `AdminDeleteModal.razor` components | Reusable, testable, keeps parent page clean |

## Data Flow

```
Browser (/admin)
  │
  ├─ GET /api/admin/dashboard ──→ AdminDashboardHandler.GetMetricsAsync()
  │                                    │
  │                                    ├─ COUNT(Products)
  │                                    ├─ COUNT(ProductVariants)
  │                                    ├─ COUNT(Variants WHERE Stock>0 AND Stock<=MinimumStockLevel)
  │                                    └─ SUM(Price * Stock)
  │
  ├─ GET /api/admin/products?status=Low+Stock ──→ AdminDashboardHandler.GetProductsAsync(status)
  │                                                    │
  │                                                    └─ Projects variants with computed StockStatus
  │                                                       (client-side filter after fetch)
  │
  ├─ PUT /api/products/{id} ──→ ProductHandler.UpdateAsync()  (reuse existing)
  └─ DELETE /api/products/{id} ──→ ProductHandler.DeleteAsync()  (reuse existing)
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `Domain/ProductVariant.cs` | Modify | Add `int MinimumStockLevel { get; set; }` |
| `Infrastructure/Data/Configurations/ProductVariantConfiguration.cs` | Modify | Add `.HasDefaultValue(5)` for `MinimumStockLevel` |
| `Features/Catalog/Products/ProductModels.cs` | Modify | Add `MinimumStockLevel` to `VariantSummary` (positional param) and `CreateVariantRequest` (optional, default 5) |
| `Features/Catalog/Products/ProductHandler.cs` | Modify | Map `MinimumStockLevel` in `ToResponse` and `CreateAsync`; add `StockStatus` to projection |
| `Features/Admin/Dashboard/AdminModels.cs` | Create | `DashboardMetrics`, `AdminProductResponse`, `AdminVariantSummary` (with `StockStatus`) |
| `Features/Admin/Dashboard/AdminDashboardHandler.cs` | Create | `GetMetricsAsync()` and `GetProductsAsync(string? statusFilter)` |
| `Features/Admin/Dashboard/AdminEndpoints.cs` | Create | `IEndpointDefinition` — maps `/api/admin/dashboard` and `/api/admin/products`, both `RequireRole("Admin")` |
| `Components/Pages/AdminDashboard.razor` | Create | `@page "/admin"`, `@rendermode InteractiveServer`, `[Authorize(Roles="Admin")]` — summary cards, filter chips, product table, modal triggers |
| `Components/Pages/AdminEditModal.razor` | Create | Form for product-level fields (name, description, category, image URLs); calls `PUT /api/products/{id}` |
| `Components/Pages/AdminDeleteModal.razor` | Create | Confirmation dialog; calls `DELETE /api/products/{id}` |
| `Components/Layout/NavMenu.razor` | Modify | Add `<AuthorizeView Roles="Admin"><Authorized><a href="/admin">Admin</a></Authorized></AuthorizeView>` |
| `Infrastructure/Data/Migrations/*` | Create | EF Core migration for `MinimumStockLevel` column |

## Interfaces / Contracts

```csharp
// AdminModels.cs
public record DashboardMetrics(int TotalProducts, int TotalSkus, int LowStockCount, decimal InventoryValue);

public record AdminVariantSummary(int Id, string Name, decimal Price, int Stock, int MinimumStockLevel, string StockStatus);

public record AdminProductResponse(
    int Id, string Name, string? Description, int CategoryId, string CategoryName,
    IEnumerable<string> ImageUrls, IEnumerable<AdminVariantSummary> Variants);
```

StockStatus computation (inline in LINQ or post-projection):
```csharp
static string ComputeStatus(int stock, int min) => stock switch
{
    0 => "Out of Stock",
    <= min => "Low Stock",
    _ => "In Stock"
};
```

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Unit | `ComputeStatus` switch logic for boundary values (0, min, min+1) | xUnit theory with inline data |
| Unit | `GetMetricsAsync` with in-memory `AppDbContext` — verify aggregates | xUnit + EF Core InMemory or SQLite |
| Integration | `GET /api/admin/dashboard` returns 401 for unauthenticated, 403 for non-Admin | WebApplicationFactory |
| Integration | `GET /api/admin/products?status=Low+Stock` filters correctly | WebApplicationFactory |
| Manual | Blazor page renders cards, filters, modals | Browser verification |

## Migration / Rollout

1. `dotnet ef migrations add AddMinimumStockLevel` — adds nullable int column with default 5.
2. Existing rows get default value 5 via migration `defaultValue: 5`.
3. No data backfill needed — all existing variants treated as `In Stock` (assuming Stock > 5).
4. Deploy API + Blazor together; no feature flag needed (admin-only route).

## Open Questions

- [ ] Should `MinimumStockLevel` be editable from the admin UI in a future iteration? (Currently only settable on variant creation.)
- [ ] Confirm `Admin` role claim is present in the auth cookie/JWT for the Blazor `AuthorizeView` to work correctly.
