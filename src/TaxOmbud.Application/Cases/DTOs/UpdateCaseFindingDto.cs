namespace TaxOmbud.Application.Cases.DTOs;

public record UpdateCaseFindingCommand(Guid CaseId, Guid FindingId, string Description) ;

public record UpdateCaseFindingRequest(string Description);
