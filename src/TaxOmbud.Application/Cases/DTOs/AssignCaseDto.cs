namespace TaxOmbud.Application.Cases.DTOs;

public record AssignCaseCommand(Guid CaseId, Guid OfficerId) ;

public record AssignCaseRequest(Guid OfficerId);
