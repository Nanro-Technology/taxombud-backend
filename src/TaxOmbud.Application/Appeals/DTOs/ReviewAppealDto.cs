namespace TaxOmbud.Application.Appeals.DTOs;

public record ReviewAppealCommand(Guid AppealId, string Action, string Notes) ;

public record ReviewAppealRequest(string Action, string Notes);
