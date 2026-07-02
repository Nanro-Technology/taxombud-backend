namespace TaxOmbud.Application.Appeals.DTOs;

public record GetAppealsQuery(
    string? Status,
    int Page = 1,
    int PageSize = 20
) ;

public record AppealListDto(
    Guid Id,
    Guid CaseId,
    string CaseNumber,
    string CaseSubject,
    string Reason,
    string Status,
    Guid? ReviewedByUserId,
    DateTimeOffset? ReviewedAt,
    DateTimeOffset CreatedAt
);