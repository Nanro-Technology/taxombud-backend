namespace TaxOmbud.Application.Cases.DTOs;

public record GetQueueQuery(
    string QueueName,
    int Page = 1,
    int PageSize = 20
) ;

public record QueueResultDto(
    string Queue,
    IEnumerable<QueueItemDto> Items,
    int Total,
    int Page,
    int PageSize
);

public record QueueItemDto(
    Guid Id,
    string ReferenceNumber,
    string Subject,
    string TaxType,
    string ComplaintCategory,
    string Status,
    string CurrentStage,
    string TaxpayerName,
    string AssignedOfficerName,
    DateTimeOffset CreatedAt
);
