namespace ShopEaseApp.Api.Features.Identity.ChangePassword;

/// <summary>
/// Request payload for the authenticated password change endpoint.
/// </summary>
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
