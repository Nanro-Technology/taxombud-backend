namespace TaxOmbud.Application.Users.DTOs;

public record AssignRolesCommand(Guid Id, Guid[] RoleIds) ;

public record AssignRolesRequest(Guid[] RoleIds);
