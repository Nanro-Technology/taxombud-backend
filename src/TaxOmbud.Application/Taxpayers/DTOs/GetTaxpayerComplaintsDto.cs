namespace TaxOmbud.Application.Taxpayers.DTOs;

public record GetTaxpayerComplaintsQuery(
    Guid TaxpayerId,
    string? Status = null,
    int Page = 1,
    int PageSize = 20
) ;
