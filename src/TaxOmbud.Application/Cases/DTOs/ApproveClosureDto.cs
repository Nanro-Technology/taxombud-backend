namespace TaxOmbud.Application.Cases.DTOs;

public record ApproveClosureCommand(Guid CaseId, bool Approve, string Rationale) ;

public record ApproveClosureRequest(bool Approve, string Rationale);