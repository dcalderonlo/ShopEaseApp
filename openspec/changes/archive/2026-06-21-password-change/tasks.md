# Tasks: Password Change Flow

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | ~150 |
| 400-line budget risk | Low |
| Chained PRs recommended | No |
| Suggested split | Single PR |
| Delivery strategy | ask-on-risk |
| Chain strategy | pending |

Decision needed before apply: No
Chained PRs recommended: No
Chain strategy: pending
400-line budget risk: Low

## Phase 1: Backend — Models & Domain (RED→GREEN)

- [x] 1.1 **RED**: Write test asserting `AppUser.MustChangePassword` defaults to `false`
- [x] 1.2 **GREEN**: Add `public bool MustChangePassword { get; set; }` to `src/ShopEaseApp.Api/Infrastructure/Data/AppUser.cs`
- [x] 1.3 **RED**: Write test for `ChangePasswordRequest`/`ChangePasswordResponse` records
- [x] 1.4 **GREEN**: Create `src/ShopEaseApp.Api/Features/Identity/ChangePassword/ChangePasswordModels.cs` with request/response records

## Phase 2: Backend — Handler & Endpoint (RED→GREEN)

- [x] 2.1 **RED**: Write unit tests for `ChangePasswordHandler` (success, wrong password, weak password)
- [x] 2.2 **GREEN**: Create `src/ShopEaseApp.Api/Features/Identity/ChangePassword/ChangePasswordHandler.cs` — inject `UserManager<AppUser>`, call `ChangePasswordAsync`, clear flag on success
- [x] 2.3 **RED**: Write integration test for `POST /api/auth/change-password` (authorized, unauthorized, invalid payloads)
- [x] 2.4 **GREEN**: Create `src/ShopEaseApp.Api/Features/Identity/ChangePassword/ChangePasswordEndpoint.cs` — `IEndpointDefinition`, `[Authorize]`, `POST /api/auth/change-password`

## Phase 3: Backend — Login Response & Seeding

- [x] 3.1 Extend `LoginResponse` in `src/ShopEaseApp.Api/Features/Identity/Login/LoginModels.cs` — add `bool MustChangePassword`
- [x] 3.2 Update `src/ShopEaseApp.Api/Features/Identity/Login/LoginHandler.cs` — pass `user.MustChangePassword` into response
- [x] 3.3 Update `src/ShopEaseApp.Api/Infrastructure/Data/Seeding/AdminSeeder.cs` — set `MustChangePassword = true` for seeded admin

## Phase 4: Frontend — Blazor Components

- [x] 4.1 Create `src/ShopEaseApp.Api/Features/Identity/Components/ChangePassword.razor` — `@page "/change-password"`, `EditForm` with current/new password, call handler
- [x] 4.2 Update `src/ShopEaseApp.Api/Features/Identity/Components/Login.razor` — after login, redirect to `/change-password` when `MustChangePassword` is true

## Phase 5: Verify

- [x] 5.1 `dotnet build` — zero warnings
- [x] 5.2 `dotnet test` — all GREEN
