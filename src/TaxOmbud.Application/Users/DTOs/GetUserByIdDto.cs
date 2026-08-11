namespace TaxOmbud.Application.Users.DTOs;

public record GetUserByIdQuery(Guid Id);

public record UserDetailDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? Phone,
    string? AltPhone,
    string? JobTitle,
    string? EmploymentType,
    DepartmentDetailDto? Department,
    string Status,
    bool CanSignIn,
    /// <summary>The single assigned role (Estate Management pattern).</summary>
    RoleDetailDto? Role,
    string UserType,
    IReadOnlyList<string>? Permissions = null
);

public record DepartmentDetailDto(Guid Id, string Name);

/// <summary>Compact role DTO returned on user responses.</summary>
public record RoleDetailDto(Guid Id, string Name, bool IsSystemRole, IReadOnlyList<string>? Permissions = null);
