namespace TaxOmbud.Application.Cases.DTOs;

public record GetCaseDocumentsQuery(Guid CaseId) ;

public record CaseDocumentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSize,
    DateTimeOffset UploadedAt
);
