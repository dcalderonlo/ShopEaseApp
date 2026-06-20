namespace ShopEaseApp.Api.Features.Identity.Register;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password);

public record RegisterResponse(string UserId, string Email, string Role);
