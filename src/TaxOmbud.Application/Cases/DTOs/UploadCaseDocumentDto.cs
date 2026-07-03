namespace TaxOmbud.Application.Cases.DTOs;

public record UploadCaseDocumentCommand(
    Guid CaseId,
    IFormFile File
) ;
