namespace TaxOmbud.Application.Cases.DTOs;

public record SubmitPublicCaseCommand(
    string Email,
    string Description,
    string? SubmitterType = null, // Personal or Corporate
    string? FirstName = null,
    string? LastName = null,
    string? Phone = null,
    string? CountryId = null,
    string? StateId = null,
    string? Subject = null,
    string? ComplaintType = null,
    string? ServiceDomain = null,
    string? Priority = null,
    string? Nin = null,
    string? OrgName = null,
    string? OrgEmail = null,
    string? OrgPhone = null,
    string? TaxId = null,
    string? CacNumber = null,
    string? OtoReason = null,
    string? CourtTribunal = null,
    IReadOnlyList<Microsoft.AspNetCore.Http.IFormFile>? Attachments = null
);


public record SubmitPublicCaseResponse(Guid CaseId, string TrackingNumber);

