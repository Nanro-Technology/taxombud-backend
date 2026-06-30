using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Taxpayers.DTOs;

public record GetTaxpayerByIdQuery(Guid Id) ;

public record TaxpayerDetailDto(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? Phone,
    string TaxpayerType,
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
    bool IsVerified,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);