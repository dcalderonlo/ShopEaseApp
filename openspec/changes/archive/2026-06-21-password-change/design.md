# Design: Password Change Flow

## Technical Approach

Extend the existing Identity feature with a forced password change mechanism. Add a `MustChangePassword` flag to `AppUser`, propagate it through `LoginResponse`, and gate Blazor navigation on it. The change-password API follows the project's Handler + Endpoint + Models pattern under `Features/Identity/ChangePassword/`.

## Architecture Decisions

| Decision | Choice | Alternatives Considered | Rationale |
|----------|--------|------------------------|-----------|
| Where to put `MustChangePassword` | `AppUser` (Identity column) | Separate table, claims | Simplest; EF migration adds one column; survives across sessions |
| Endpoint verb | `POST /api/auth/change-password` | PUT | Proposal says PUT but project convention uses POST for auth actions (login, register, logout). Consistency wins. |
| Handler pattern | `ChangePasswordHandler` with `HandleAsync` returning tuple | Controller-based | Matches existing Login/Register handler pattern exactly |
| Blazor redirect location | `Login.razor` `HandleLogin` method | Middleware, AuthStateProvider | Minimal blast radius; flag is already in `LoginResponse`; no extra round-trip |
| Clearing the flag | `ChangePasswordHandler` sets `MustChangePassword = false` after success | Separate endpoint | Atomic: change + clear in one operation prevents orphaned flags |

## Data Flow

```
Login.razor ──POST──→ LoginHandler ──→ LoginResponse { MustChangePassword }
                                              │
                                    ┌─ true ──┴── false ──┐
                                    ▼                      ▼
                          /change-password               / (home)
                                    │
                          ChangePassword.razor
                                    │
                          POST /api/auth/change-password
                                    │
                          ChangePasswordHandler
                          ┌─────────┴──────────┐
                          ▼                     ▼
                   UserManager             MustChangePassword
                  .ChangePasswordAsync      = false (save)
                          │
                    ┌─ OK ┴── Fail ──┐
                    ▼                 ▼
               NavigateTo("/")    Show errors
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `src/ShopEaseApp.Api/Infrastructure/Data/AppUser.cs` | Modify | Add `public bool MustChangePassword { get; set; }` (default `false`) |
| `src/ShopEaseApp.Api/Features/Identity/Login/LoginModels.cs` | Modify | Add `bool MustChangePassword` to `LoginResponse` record |
| `src/ShopEaseApp.Api/Features/Identity/Login/LoginHandler.cs` | Modify | Pass `user.MustChangePassword` into `LoginResponse` constructor |
| `src/ShopEaseApp.Api/Features/Identity/ChangePassword/ChangePasswordModels.cs` | Create | `ChangePasswordRequest(string CurrentPassword, string NewPassword)` and `ChangePasswordResponse(bool Success, string[] Errors)` |
| `src/ShopEaseApp.Api/Features/Identity/ChangePassword/ChangePasswordHandler.cs` | Create | Injects `UserManager<AppUser>`, resolves user from `HttpContext`, calls `ChangePasswordAsync`, sets `MustChangePassword = false` on success |
| `src/ShopEaseApp.Api/Features/Identity/ChangePassword/ChangePasswordEndpoint.cs` | Create | `IEndpointDefinition` implementation; `POST /api/auth/change-password`, `[Authorize]` |
| `src/ShopEaseApp.Api/Features/Identity/Components/Login.razor` | Modify | After successful login, check `response.MustChangePassword`; if true, navigate to `/change-password` instead of `/` |
| `src/ShopEaseApp.Api/Features/Identity/Components/ChangePassword.razor` | Create | `@page "/change-password"`, `EditForm` with current + new password fields, calls `ChangePasswordHandler` directly (same pattern as Login.razor) |
| `src/ShopEaseApp.Api/Infrastructure/Data/Seeding/AdminSeeder.cs` | Modify | Set `MustChangePassword = true` on the admin `AppUser` before `CreateAsync` |

## Interfaces / Contracts

```csharp
// ChangePasswordModels.cs
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record ChangePasswordResponse(bool Success, string[] Errors);

// LoginResponse — extended
public record LoginResponse(string Token, string Email, string Role, DateTime ExpiresAt, bool MustChangePassword);
```

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Unit | `ChangePasswordHandler` — success path, wrong current password, weak new password | Mock `UserManager<AppUser>`, assert tuple results |
| Unit | `LoginHandler` — `MustChangePassword` propagates to response | Existing test pattern + assert new field |
| Integration | `POST /api/auth/change-password` — authorized access, valid/invalid payloads | `WebApplicationFactory`, seeded test DB |

## Migration / Rollout

EF migration adds `MustChangePassword` column (default `false`). Existing users are unaffected — they keep `false` and skip the redirect. Only the seeded admin gets `true`. No feature flag needed; rollback is a migration revert + code revert.

## Open Questions

- None
