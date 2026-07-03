namespace TaxOmbud.Application.Users.DTOs;

public record UpdateCurrentUserCommand(
    string FirstName,
    string LastName,
    string? Phone,
    string? JobTitle
) ;

public record UpdateCurrentUserRequest(string FirstName, string LastName, string? Phone, string? JobTitle);
