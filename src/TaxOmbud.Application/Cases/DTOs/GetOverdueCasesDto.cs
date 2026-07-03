namespace TaxOmbud.Application.Cases.DTOs;

public record GetOverdueCasesQuery(
    int Page = 1,
    int PageSize = 20) ;
