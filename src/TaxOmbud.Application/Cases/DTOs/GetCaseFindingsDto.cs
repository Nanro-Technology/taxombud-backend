namespace TaxOmbud.Application.Cases.DTOs;

public record GetCaseFindingsQuery(Guid CaseId) ;

public record CaseFindingDto(
    Guid Id,
    Guid CaseId,
    string Description,
    DateTimeOffset CreatedAt,
    Guid? CreatedBy
);
