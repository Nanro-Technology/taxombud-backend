namespace TaxOmbud.Application.Users.DTOs;

public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Phone,
    string? JobTitle,
    string? EmploymentType,
    Guid? DepartmentId
) ;

public record CreateUserResponse(Guid Id, string FullName, string Email);
