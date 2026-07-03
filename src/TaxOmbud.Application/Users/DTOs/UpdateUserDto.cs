namespace TaxOmbud.Application.Users.DTOs;

public record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string? Phone,
    string? JobTitle,
    string? EmploymentType,
    Guid? DepartmentId
) ;

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    string? Phone,
    string? JobTitle,
    string? EmploymentType,
    Guid? DepartmentId
);
