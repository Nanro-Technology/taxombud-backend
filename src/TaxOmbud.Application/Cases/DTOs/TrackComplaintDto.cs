namespace TaxOmbud.Application.Cases.DTOs;

public record TrackComplaintQuery(string TrackingNumber);

public record TrackComplaintResponse(
    string TrackingNumber,
    string Status,
    string CurrentStage,
    string Description,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? UpdatedAt
);