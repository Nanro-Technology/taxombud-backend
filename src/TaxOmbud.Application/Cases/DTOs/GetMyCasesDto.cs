namespace TaxOmbud.Application.Cases.DTOs;

public record GetMyCasesQuery(
    string? Search,
    string? Stage,
    string? Status,
    int Page = 1,
    int PageSize = 20
) ;