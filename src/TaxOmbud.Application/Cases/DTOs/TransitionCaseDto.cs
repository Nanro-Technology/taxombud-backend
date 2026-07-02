namespace TaxOmbud.Application.Cases.DTOs;

public record TransitionCaseCommand(Guid CaseId, string TargetStage, string? Reason) ;

public record TransitionCaseRequest(string TargetStage, string? Reason);