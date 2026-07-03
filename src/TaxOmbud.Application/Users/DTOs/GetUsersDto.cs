namespace TaxOmbud.Application.Users.DTOs;

public record GetUsersQuery(
    string? Search,
    string? Status,
    Guid? DepartmentId,
    int Page = 1,
    int PageSize = 20
) ;

public record UserListDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? Phone,
    string? JobTitle,
    string? EmploymentType,
    DepartmentDto? Department,
    string Status,
    bool CanSignIn,
    /// <summary>Single assigned role (Estate Management pattern: one role per user).</summary>
    RoleDto? Role,
    string UserType
);

public record DepartmentDto(Guid Id, string Name);
public record RoleDto(Guid Id, string Name);
