namespace TaxOmbud.Application.Cases.DTOs;

public record AddCaseFindingCommand(Guid CaseId, string Description) ;

public record AddCaseFindingRequest(string Description);