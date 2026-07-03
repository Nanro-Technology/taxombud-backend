namespace TaxOmbud.Application.Users.DTOs;

public record UpdateUserStatusCommand(Guid Id, bool Activate) ;

public record UpdateUserStatusRequest(bool Activate);
