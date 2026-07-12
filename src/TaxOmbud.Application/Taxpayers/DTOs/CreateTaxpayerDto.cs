using System;

namespace TaxOmbud.Application.Taxpayers.DTOs;

public record CreateTaxpayerCommand(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? AltPhone,
    string? TinNumber,
    string? Nin,
    string? Bvn,
    string? Gender,
    string? CompanyName,
    string? RcNumber,
    string? Address,
    string? City,
    string? State,
    string? Country,
    string? Account
);

public record CreateTaxpayerRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? AltPhone,
    string? TinNumber,
    string? Nin,
    string? Bvn,
    string? Gender,
    string? CompanyName,
    string? RcNumber,
    string? Address,
    string? City,
    string? State,
    string? Country,
    string? Account
);
