namespace TaxOmbud.Application.Taxpayers.DTOs;

public record UpdateTaxpayerCommand(
    Guid TaxpayerId,
    string FirstName,
    string LastName,
    string Phone,
    string? AltPhone,
    string? TinNumber,
    string? Nin,
    string? Bvn,
    string? Gender,
    DateTimeOffset? DateOfBirth,
    string? CompanyName,
    string? RcNumber,
    string? Address,
    string? City,
    string? State,
    string? Country,
    string? Account
) ;

public record UpdateTaxpayerRequest(
    string FirstName,
    string LastName,
    string Phone,
    string? AltPhone,
    string? TinNumber,
    string? Nin,
    string? Bvn,
    string? Gender,
    DateTimeOffset? DateOfBirth,
    string? CompanyName,
    string? RcNumber,
    string? Address,
    string? City,
    string? State,
    string? Country,
    string? Account
);
