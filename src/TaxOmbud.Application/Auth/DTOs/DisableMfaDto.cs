namespace TaxOmbud.Application.Auth.DTOs;

public record DisableMfaCommand(Guid UserId, string Password) ;

public record DisableMfaRequest(string Password);
