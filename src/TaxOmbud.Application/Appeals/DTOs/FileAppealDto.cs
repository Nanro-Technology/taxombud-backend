namespace TaxOmbud.Application.Appeals.DTOs;

public record FileAppealCommand(Guid CaseId, string Reason) ;

public record FileAppealRequest(Guid CaseId, string Reason);

public record FileAppealResponse(Guid Id, Guid CaseId, string Reason, DateTimeOffset CreatedAt);