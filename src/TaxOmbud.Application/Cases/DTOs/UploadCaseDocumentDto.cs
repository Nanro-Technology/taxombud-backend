using Microsoft.AspNetCore.Http;

namespace TaxOmbud.Application.Cases.DTOs;

public record UploadCaseDocumentCommand(
    Guid CaseId,
    IFormFile File
) ;

/// <summary>Bulk-upload multiple documents to a case in a single call.</summary>
public record UploadCaseDocumentsCommand(
    Guid CaseId,
    IReadOnlyList<IFormFile> Files
);
