namespace TaxOmbud.Application.Cases.DTOs;

public record GetCaseCommunicationsQuery(Guid CaseId) ;

public record CaseCommunicationDto(
    Guid Id,
    Guid CaseId,
    string Sender,
    string Recipient,
    string Direction,
    string Subject,
    string Body,
    DateTimeOffset SentAt,
    string Channel
);
