namespace TaxOmbud.Application.Appeals.DTOs;

public record GetAppealByIdQuery(Guid Id) ;

public record AppealDetailDto(
    Guid Id,
    Guid CaseId,
    string CaseNumber,
    string CaseSubject,
    string Reason,
    string Status,
    Guid? ReviewedByUserId,
    string? ReviewNote,
    DateTimeOffset? ReviewedAt,
    DateTimeOffset CreatedAt,
    IEnumerable<AppealStatusHistoryDto> StatusHistory
);

public record AppealStatusHistoryDto(
    Guid Id,
    string PreviousStatus,
    string NewStatus,
    string? Notes,
    Guid ChangedByUserId,
    DateTimeOffset CreatedAt
);
