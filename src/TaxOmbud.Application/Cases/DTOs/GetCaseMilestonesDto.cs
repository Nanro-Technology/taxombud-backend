namespace TaxOmbud.Application.Cases.DTOs;

public record GetCaseMilestonesQuery(Guid CaseId) ;

public record CaseMilestoneDto(
    Guid Id,
    Guid CaseId,
    string Title,
    string? Description,
    DateTimeOffset? TargetDate,
    DateTimeOffset? CompletedAt,
    bool IsCompleted
);