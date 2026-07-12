using System;

namespace TaxOmbud.Application.Cases.DTOs;

public record GetCaseHistoryQuery(
    string? Search,
    string? DateFrom,
    string? DateTo,
    int Page = 1,
    int PageSize = 20
) ;

public record CaseHistoryListDto(
    Guid Id,
    Guid CaseId,
    string CaseNumber,
    string Subject,
    string TaxType,
    string Activity,
    string Status,
    DateTimeOffset TransitionedAt,
    string? Reason,
    string OperatorName
);
