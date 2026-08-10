namespace TaxOmbud.Application.Cases.DTOs;

public record SubmitPublicCaseCommand(
    string SubmitterType, // Personal or Corporate
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string CountryId,
    string StateId,
    string Description,
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
    string? CourtTribunal = null
);

public record SubmitPublicCaseResponse(Guid CaseId, string TrackingNumber);

