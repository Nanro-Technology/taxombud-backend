namespace TaxOmbud.Application.Auth.DTOs;

public record SetupMfaCommand(Guid UserId) ;

public record SetupMfaResponse(
    string QrCodeUri,
    string SecretKey,
    IReadOnlyList<string> BackupCodes
);