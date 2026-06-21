namespace ShopEaseApp.Api.Features.Identity.Login;

public record LoginRequest(string Email, string Password);

/// <summary>
/// Login response. <paramref name="MustChangePassword"/> defaults to false so
/// existing callers (and clients that ignore it) are unaffected; the flag drives
/// the forced first-change redirect in the Blazor login flow.
/// </summary>
public record LoginResponse(string Token, string Email, string Role, DateTime ExpiresAt, bool MustChangePassword = false);
