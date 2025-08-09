namespace Ecommerce.Api.DTOs;

public record RegisterDto(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber
);

public record LoginDto(string Email, string Password);

public record AuthResponse(string Token);
