namespace TaxOmbud.Application.Appeals.DTOs;

public record GetAppealDocumentsQuery(Guid AppealId) ;

public record AppealDocumentDto(Guid Id, string FileName, string ContentType, long FileSize, DateTimeOffset CreatedAt);