namespace TaxOmbud.Application.Appeals.DTOs;

public record UploadAppealDocumentCommand(
    Guid AppealId,
    IFormFile File
) ;