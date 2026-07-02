namespace TaxOmbud.Application.Auth.DTOs;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber
) ;

public record RegisterResponse(Guid UserId, string Email, string FullName);