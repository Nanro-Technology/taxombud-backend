namespace TaxOmbud.Application.Users.DTOs;

public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Password = null,
    string? Phone = null,
    string? JobTitle = null,
    string? EmploymentType = null,
    Guid? DepartmentId = null,
    Guid? RoleId = null
);

public record CreateUserResponse(Guid Id, string FullName, string Email);
