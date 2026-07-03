namespace TaxOmbud.Application.Users.DTOs;

public record ApplyPermissionOverridesCommand(Guid Id, PermissionOverrideDto[] Overrides) ;

public record PermissionOverrideDto(string PermissionCode, string Mode);

public record PermissionOverridesRequest(PermissionOverrideDto[] Overrides);
