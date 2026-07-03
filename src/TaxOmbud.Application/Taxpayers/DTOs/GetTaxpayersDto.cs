namespace TaxOmbud.Application.Taxpayers.DTOs;

public record GetTaxpayersQuery(
    string? Search,
    string? Type,
    bool? IsVerified,
    int Page = 1,
    int PageSize = 20
) ;

public record TaxpayerListDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string? Phone,
    string TaxpayerType,
    string? TinNumber,
    string? Nin,
    string? Bvn,
    string? CompanyName,
    string? RcNumber,
    bool IsVerified,
    DateTimeOffset CreatedAt
);
