# Proposal: Password Change Flow

## Intent

Add an authenticated password change endpoint and a forced first-change flow for seeded admin accounts to improve the application's security posture.

## Scope

### In Scope
- **PUT /api/auth/change-password**: authenticated endpoint requiring `currentPassword` and `newPassword`
- **Blazor page `/change-password`**: simple form that redirects after success
- **Admin forced first-change**: add `MustChangePassword` bool to `AppUser`; `LoginHandler` returns the flag in `LoginResponse`
- **Blazor redirect**: login page checks `MustChangePassword` and routes to `/change-password` instead of `/`
- **AdminSeeder**: sets `MustChangePassword=true` for the default seeded admin

### Out of Scope
- Password reset via email (no email sender configured)
- Password strength meter in UI (deferred styling)
- Forcing change for existing non-admin users

## Capabilities

### New Capabilities
- None

### Modified Capabilities
- `identity`: ADD password change endpoint and forced first-change flow

## Approach

Extend `AppUser` with a `MustChangePassword` flag. Update `LoginHandler` to inspect the flag and include it in `LoginResponse`. Add a `ChangePassword` API endpoint backed by `UserManager.ChangePasswordAsync`. In Blazor, read the flag after login and redirect before the normal home-page navigation. Update `AdminSeeder` to set the flag for the default admin account.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `Domain/Entities/AppUser.cs` | Modified | Add `MustChangePassword` bool |
| `Application/Handlers/LoginHandler.cs` | Modified | Return `MustChangePassword` flag |
| `API/Controllers/AuthController.cs` | Modified | Add `ChangePassword` endpoint |
| `Shared/Contracts/LoginResponse.cs` | Modified | Add `MustChangePassword` property |
| `Blazor/Pages/ChangePassword.razor` | New | Change password form page |
| `Blazor/Pages/Login.razor` | Modified | Redirect to `/change-password` when flagged |
| `Infrastructure/Data/AdminSeeder.cs` | Modified | Set `MustChangePassword=true` for seeded admin |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Breaking login response contract | Low | Add optional bool; existing clients ignore it |
| Admin lockout if change fails | Low | Endpoint uses standard `ChangePasswordAsync` validation |

## Rollback Plan

Revert the `MustChangePassword` property and seeding, remove the endpoint and Blazor page, and restore `LoginHandler` to the previous response shape.

## Dependencies

None.

## Success Criteria

- [ ] Authenticated users can change password via API and Blazor UI
- [ ] Seeded admin logs in and is redirected to change password
- [ ] After changing password, admin can access the full application
