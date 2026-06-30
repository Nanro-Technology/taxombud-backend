namespace TaxOmbud.Application.Auth.DTOs;

public record VerifyMfaCommand(Guid UserId, string TotpCode) ;

public record VerifyMfaRequest(string TotpCode);