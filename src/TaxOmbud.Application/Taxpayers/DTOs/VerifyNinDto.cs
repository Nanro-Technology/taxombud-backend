namespace TaxOmbud.Application.Taxpayers.DTOs;

public record VerifyNinQuery(string Nin) ;

public record NinVerificationResponseDto(
    bool Verified,
    string Nin,
    string FirstName,
    string LastName,
    string DateOfBirth,
    string Gender,
    string PhotoBase64
);

public record NinVerificationRequest(string Nin);
